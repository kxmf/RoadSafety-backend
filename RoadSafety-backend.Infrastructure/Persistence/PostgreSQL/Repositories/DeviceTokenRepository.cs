using Microsoft.EntityFrameworkCore;
using RoadSafety_backend.Domain.Aggregates.DeviceTokenAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;

public sealed class DeviceTokenRepository(ApplicationDbContext dbContext) : IDeviceTokenRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<DeviceToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _dbContext.DeviceTokens
            .FirstOrDefaultAsync(deviceToken => deviceToken.Token == token, cancellationToken);
    }

    public async Task<List<DeviceToken>> GetActiveByUserIdsAsync(IEnumerable<UserId> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
            return [];

        var tokens = new List<DeviceToken>();
        foreach (var userId in ids)
        {
            tokens.AddRange(await _dbContext.DeviceTokens
                .Where(deviceToken => deviceToken.UserId == userId && deviceToken.RevokedAt == null)
                .ToListAsync(cancellationToken));
        }

        return tokens;
    }

    public async Task AddAsync(DeviceToken deviceToken, CancellationToken cancellationToken)
    {
        await _dbContext.DeviceTokens.AddAsync(deviceToken, cancellationToken);
    }
}
