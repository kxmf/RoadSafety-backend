using Microsoft.Extensions.Options;
using RoadSafety_backend.Application.DTOs.Responses.Tracking;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.TrackingAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Tracking;

public class GetChildrenLocationsUseCase(
    IFamilyRepository familyRepository,
    IUserRepository userRepository,
    ITrackingRepository trackingRepository,
    ICurrentUserAccessor userAccessor,
    IOptions<TrackingOptions> options)
{
    public async Task<Result<ChildLocationsResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var access = await FamilyTrackingAccessValidator.ValidateFamilyMemberAsync(familyRepository, userAccessor, cancellationToken);
        if (!access.IsSuccess)
            return Result<ChildLocationsResponse>.Failure(access.Error);

        var childrenResult = FamilyTrackingAccessValidator.ValidateParentChildrenRead(access.Value.Family, access.Value.CurrentMember);
        if (!childrenResult.IsSuccess)
            return Result<ChildLocationsResponse>.Failure(childrenResult.Error);

        var childIds = childrenResult.Value.Select(member => member.UserId).ToList();
        if (childIds.Count == 0)
            return Result<ChildLocationsResponse>.Success(new ChildLocationsResponse([]));

        var locations = await trackingRepository.GetLocationsAsync(childIds, cancellationToken);
        var childUsers = new Dictionary<UserId, User>();
        foreach (var childId in childIds)
        {
            var user = await userRepository.GetUserByIdAsync(childId, cancellationToken);
            if (user is not null)
                childUsers[childId] = user;
        }

        var minLastUpdatedAt = DateTimeOffset.UtcNow.AddMinutes(-Math.Max(1, options.Value.ChildLocationFreshnessMinutes));
        var responses = locations
            .Where(location => location.LastUpdatedAt >= minLastUpdatedAt)
            .GroupBy(location => location.ChildId)
            .Select(group => group.OrderByDescending(location => location.LastUpdatedAt).First())
            .Select(location =>
            {
                childUsers.TryGetValue(location.ChildId, out var child);
                return TrackingResponseMapper.ToResponse(location, child);
            })
            .ToList();

        return Result<ChildLocationsResponse>.Success(new ChildLocationsResponse(responses));
    }
}
