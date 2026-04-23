namespace RoadSafety_backend.Domain.Aggregates.SessionAggregate;

public interface ISessionRepository
{
    public Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    public Task<Session> CreateSessionAsync(Session session, CancellationToken cancellationToken);
}
