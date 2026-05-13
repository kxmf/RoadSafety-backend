using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.SessionAggregate;

public class RefreshToken
{
    public RefreshTokenId Id { get; init; }

    public string TokenHash { get; init; }

    public UserId UserId { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }

    public bool IsUsed { get; private set; }
    public bool IsRevoked { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsUsed && DateTime.UtcNow < ExpiresAt;

    public RefreshTokenId? ReplacedByTokenId { get; private set; }

    private RefreshToken() { }

    private RefreshToken(RefreshTokenId id, string tokenHash, UserId userId, DateTimeOffset expiresAt)
    {
        Id = id;
        TokenHash = tokenHash;
        UserId = userId;
        ExpiresAt = expiresAt;
        IsUsed = false;
        IsRevoked = false;
    }

    public static RefreshToken Create(RefreshTokenId id, string tokenHash, UserId userId, DateTimeOffset expiresAt)
    {
        return new RefreshToken(id, tokenHash, userId, expiresAt);
    }

    public void MarkAsUsed(RefreshTokenId replacedByTokenId)
    {
        IsUsed = true;
        ReplacedByTokenId = replacedByTokenId;
    }

    public void Revoke() => IsRevoked = true;
}
