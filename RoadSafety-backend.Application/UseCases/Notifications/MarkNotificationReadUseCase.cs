using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.NotificationAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Notifications;

public class MarkNotificationReadUseCase(
    INotificationRepository notificationRepository,
    ICurrentUserAccessor userAccessor,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<bool>> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<bool>.Failure(Error.Unauthorized("User not authenticated."));

        if (id == Guid.Empty)
            return Result<bool>.Failure(Error.Validation("notification id is required."));

        var notification = await notificationRepository.GetByIdAsync(id, cancellationToken);
        if (notification is null)
            return Result<bool>.Failure(Error.NotFound("Notification not found."));

        if (notification.RecipientUserId != userAccessor.UserId)
            return Result<bool>.Failure(Error.Forbidden("Notification belongs to another user."));

        notification.MarkRead(DateTimeOffset.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
