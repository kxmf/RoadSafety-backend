using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.SessionAggregate;

public class RefreshToken
{
    public RefreshTokenId Id { get; init; }

    public string TokenHash { get; init; }

    public UserId UserId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }

    public bool IsUsed { get; private set; }
    public bool IsRevoked { get; private set; }

    public RefreshTokenId? ReplacedByTokenId { get; private set; }

    public RefreshToken(RefreshTokenId id, string tokenHash, UserId userId, DateTimeOffset createdAt, DateTimeOffset expiresAt, bool isUsed, bool isRevoked, RefreshTokenId? replacedByTokenId)
    {
        Id = id;
        TokenHash = tokenHash;
        UserId = userId;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        IsUsed = isUsed;
        IsRevoked = isRevoked;
        ReplacedByTokenId = replacedByTokenId;
    }
}
