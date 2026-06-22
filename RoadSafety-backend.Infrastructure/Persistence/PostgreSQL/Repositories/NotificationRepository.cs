using Microsoft.EntityFrameworkCore;
using RoadSafety_backend.Domain.Aggregates.NotificationAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;

public sealed class NotificationRepository(ApplicationDbContext dbContext) : INotificationRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<List<Notification>> GetByRecipientAsync(UserId recipientUserId, bool unreadOnly, CancellationToken cancellationToken)
    {
        var query = _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.RecipientUserId == recipientUserId);

        if (unreadOnly)
            query = query.Where(notification => notification.ReadAt == null);

        return await query
            .OrderByDescending(notification => notification.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Notifications
            .FirstOrDefaultAsync(notification => notification.Id == id, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken)
    {
        await _dbContext.Notifications.AddRangeAsync(notifications, cancellationToken);
    }
}
