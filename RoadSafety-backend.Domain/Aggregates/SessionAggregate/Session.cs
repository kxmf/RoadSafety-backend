using System.Security;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.SessionAggregate;

public class Session
{
    public SessionId Id { get; init; } = null!;

    public UserId UserId { get; init; } = null!;

    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    public RefreshToken? CurrentRefreshToken => _refreshTokens.FirstOrDefault(t => t.IsActive);

    public bool IsRevoked { get; private set; }

    private Session() { }

    private Session(SessionId id, UserId userId, RefreshToken refreshToken)
    {
        Id = id;
        UserId = userId;
        IsRevoked = false;
        _refreshTokens = [refreshToken];
    }
    
    public static Session Create(SessionId id, UserId userId, RefreshToken refreshToken)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(refreshToken);

        if (id.Value == Guid.Empty)
            throw new ArgumentException("Session ID cannot be empty.", nameof(id));
        if (userId.Value == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        return new Session(id, userId, refreshToken);
    }
    
    public void RotateRefreshToken(RefreshToken newToken)
    {
        ArgumentNullException.ThrowIfNull(newToken);

        if (IsRevoked)
            throw new InvalidOperationException("Cannot rotate token for a revoked session.");

        var oldToken = CurrentRefreshToken;

        if (oldToken == null)
        {
            Revoke();
            throw new SecurityException("Potential token reuse detected!");
        }

        oldToken.MarkAsUsed(newToken.Id);

        _refreshTokens.Add(newToken);
    }

    public void Revoke()
    {
        IsRevoked = true;
        foreach (var token in _refreshTokens)
        {
            token.Revoke();
        }
    }
}
