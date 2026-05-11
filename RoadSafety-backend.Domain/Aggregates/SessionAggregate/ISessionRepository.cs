using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.SessionAggregate;

public interface ISessionRepository
{
    public Task<Session?> GetSessionByUserIdAsync(UserId id, CancellationToken cancellationToken);
    public Task<Session?> GetSessionByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken);
    public Task<Session> CreateSessionAsync(Session session, CancellationToken cancellationToken);
    public Task<Session> UpdateSessionAsync(Session session, CancellationToken cancellationToken);
}
