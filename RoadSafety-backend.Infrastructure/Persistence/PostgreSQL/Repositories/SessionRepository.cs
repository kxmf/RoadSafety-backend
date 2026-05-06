using Microsoft.EntityFrameworkCore;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;

public sealed class SessionRepository(ApplicationDbContext dbContext) : ISessionRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Session> GetSessionByUserIdAsync(UserId id, CancellationToken cancellationToken)
    {
        return (await _dbContext.Sessions
            .Include(s => s.RefreshTokens)
            .FirstOrDefaultAsync(s => s.UserId == id, cancellationToken))!;
    }

    public async Task<Session> GetSessionByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken)
    {
        return (await _dbContext.Sessions
            .Include(s => s.RefreshTokens)
            .FirstOrDefaultAsync(s => s.RefreshTokens.Any(rt => rt.TokenHash == refreshTokenHash), cancellationToken))!;
    }

    public async Task<Session> CreateSessionAsync(Session session, CancellationToken cancellationToken)
    {
        await _dbContext.Sessions.AddAsync(session, cancellationToken);

        return session;
    }

    public Task<Session> UpdateSessionAsync(Session session, CancellationToken cancellationToken)
    {
        _dbContext.Sessions.Update(session);

        return Task.FromResult(session);
    }
}
