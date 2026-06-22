using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.NotificationAggregate;

public interface INotificationRepository
{
    Task<List<Notification>> GetByRecipientAsync(UserId recipientUserId, bool unreadOnly, CancellationToken cancellationToken);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken);
}
