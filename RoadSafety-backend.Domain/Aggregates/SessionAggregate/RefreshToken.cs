namespace RoadSafety_backend.Domain.Aggregates.SessionAggregate;

public class RefreshToken
{
    public RefreshTokenId Id { get; init; }

    public string TokenHash { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }

    public bool IsUsed { get; private set; }
    public bool IsRevoked { get; private set; }

    public RefreshTokenId? ReplacedByTokenId { get; private set; }
}
