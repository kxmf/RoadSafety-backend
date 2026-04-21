using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.SessionAggregate;

public class Session
{
    public SessionId Id { get; init; }

    public UserId UserId { get; init; }

    public RefreshTokenId RefreshTokenId { get; private set; }
    public RefreshToken RefreshToken { get; private set; }

    public bool IsRevoked { get; private set; }
}
