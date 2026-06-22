using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using RoadSafety_backend.Application.DTOs.Requests.Tracking;
using RoadSafety_backend.Application.DTOs.Responses.Tracking;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.DeviceTokenAggregate;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.NotificationAggregate;
using RoadSafety_backend.Domain.Aggregates.TrackingAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Tracking;

public class SubmitLocationUseCase(
    IFamilyRepository familyRepository,
    IUserRepository userRepository,
    ITrackingRepository trackingRepository,
    INotificationRepository notificationRepository,
    IDeviceTokenRepository deviceTokenRepository,
    IPushNotificationSender pushNotificationSender,
    ICurrentUserAccessor userAccessor,
    IUnitOfWork unitOfWork,
    ILogger<SubmitLocationUseCase> logger,
    IOptions<TrackingOptions> options)
{
    private static readonly GeometryFactory GeometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public async Task<Result<SubmitLocationResponse>> ExecuteAsync(SubmitLocationRequest request, CancellationToken cancellationToken)
    {
        if (!IsValidCoordinate(request.Latitude, request.Longitude))
            return Result<SubmitLocationResponse>.Failure(Error.Validation("Latitude and longitude are invalid."));

        if (request.AccuracyMeters < 0)
            return Result<SubmitLocationResponse>.Failure(Error.Validation("Accuracy cannot be negative."));

        var access = await FamilyTrackingAccessValidator.ValidateFamilyMemberAsync(familyRepository, userAccessor, cancellationToken);
        if (!access.IsSuccess)
            return Result<SubmitLocationResponse>.Failure(access.Error);

        var childResult = FamilyTrackingAccessValidator.ValidateChildAccess(
            access.Value.Family,
            access.Value.CurrentMember,
            access.Value.CurrentUserId,
            request.ChildId);

        if (!childResult.IsSuccess)
            return Result<SubmitLocationResponse>.Failure(childResult.Error);

        var childId = childResult.Value;
        var now = DateTimeOffset.UtcNow;
        var recordedAt = request.RecordedAt ?? now;
        var point = GeometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));
        var match = await trackingRepository.GetRiskMatchAsync(
            access.Value.Family.Id,
            access.Value.Family.CityId,
            childId,
            point,
            cancellationToken);

        var location = ChildLocation.Create(
            childId,
            access.Value.Family.Id,
            point,
            request.AccuracyMeters,
            match.Risk,
            match.MatchedUserAreaId,
            match.MatchedBaseAreaKey,
            recordedAt,
            now);

        await trackingRepository.UpsertLocationAsync(location, cancellationToken);
        await trackingRepository.EnsureStatsAsync(childId, cancellationToken);
        var notifications = await UpdateRiskStateAndNotificationsAsync(access.Value.Family, childId, point, match.Risk, now, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        try
        {
            await SendPushNotificationsAsync(notifications, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to send push notifications for child {ChildId}", childId.Value);
        }

        return Result<SubmitLocationResponse>.Success(new SubmitLocationResponse(
            childId.Value,
            match.Risk,
            match.MatchedUserAreaId,
            match.MatchedBaseAreaKey,
            now));
    }

    private async Task<IReadOnlyCollection<Notification>> UpdateRiskStateAndNotificationsAsync(
        RoadSafety_backend.Domain.Aggregates.FamilyAggregate.Family family,
        UserId childId,
        Point point,
        RiskLevel risk,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var state = await trackingRepository.GetRiskStateAsync(childId, cancellationToken);
        if (state is null)
        {
            state = ChildRiskState.Create(childId, risk, now);
            await trackingRepository.UpsertRiskStateAsync(state, cancellationToken);
        }
        else
        {
            state.ApplyRisk(risk, now);
        }

        var cooldown = TimeSpan.FromMinutes(Math.Max(0, options.Value.RedZoneNotificationCooldownMinutes));
        if (!state.ShouldCreateRedNotification(now, cooldown))
            return [];

        var parentIds = family.Members
            .Where(member => member.Role == FamilyMemberRole.Parent)
            .Select(member => member.UserId)
            .ToList();

        if (parentIds.Count == 0)
            return [];

        var child = await userRepository.GetUserByIdAsync(childId, cancellationToken);
        var childDisplayName = TrackingResponseMapper.GetDisplayName(child);
        var notifications = parentIds.Select(parentId =>
            Notification.CreateChildEnteredRedZone(
                Guid.NewGuid(),
                parentId,
                childId,
                childDisplayName,
                point,
                now))
            .ToList();

        await notificationRepository.AddRangeAsync(notifications, cancellationToken);
        state.MarkRedNotificationCreated(now);

        return notifications;
    }

    private async Task SendPushNotificationsAsync(IReadOnlyCollection<Notification> notifications, CancellationToken cancellationToken)
    {
        if (notifications.Count == 0)
            return;

        foreach (var notification in notifications)
        {
            var deviceTokens = await deviceTokenRepository.GetActiveByUserIdsAsync([notification.RecipientUserId], cancellationToken);
            var tokens = deviceTokens.Select(deviceToken => deviceToken.Token).Distinct().ToList();
            if (tokens.Count == 0)
                continue;

            var result = await pushNotificationSender.SendAsync(notification, tokens, cancellationToken);
            if (result.InvalidTokens.Count == 0)
                continue;

            var invalidTokens = result.InvalidTokens.ToHashSet(StringComparer.Ordinal);
            foreach (var deviceToken in deviceTokens.Where(deviceToken => invalidTokens.Contains(deviceToken.Token)))
            {
                deviceToken.Revoke(DateTimeOffset.UtcNow);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static bool IsValidCoordinate(double latitude, double longitude)
    {
        return !double.IsNaN(latitude)
               && !double.IsNaN(longitude)
               && !double.IsInfinity(latitude)
               && !double.IsInfinity(longitude)
               && latitude is >= -90 and <= 90
               && longitude is >= -180 and <= 180;
    }
}
