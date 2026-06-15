using RoadSafety_backend.Application.DTOs.Responses.Notifications;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.NotificationAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Notifications;

public class GetNotificationsUseCase(
    INotificationRepository notificationRepository,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<NotificationsResponse>> ExecuteAsync(bool unreadOnly, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<NotificationsResponse>.Failure(Error.Unauthorized("User not authenticated."));

        var notifications = await notificationRepository.GetByRecipientAsync(userAccessor.UserId, unreadOnly, cancellationToken);
        return Result<NotificationsResponse>.Success(new NotificationsResponse(
            notifications.Select(NotificationResponseMapper.ToResponse).ToList()));
    }
}
