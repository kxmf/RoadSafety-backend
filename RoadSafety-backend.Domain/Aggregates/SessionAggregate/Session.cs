using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using System.Security;

namespace RoadSafety_backend.Domain.Aggregates.SessionAggregate;

public class Session
{
    public SessionId Id { get; init; }

    public UserId UserId { get; init; }

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    public RefreshToken? CurrentRefreshToken => _refreshTokens.FirstOrDefault(t => t.IsActive);

    public bool IsRevoked { get; private set; }

    private Session() { }

    private Session(SessionId id, UserId userId, RefreshToken refreshToken)
    {
        Id = id;
        UserId = userId;
        IsRevoked = false;
        _refreshTokens.Add(refreshToken);
    }

    public static Session Create(SessionId id, UserId userId, RefreshToken refreshToken)
    {
        return new Session(id, userId, refreshToken);
    }

    public void RotateRefreshToken(RefreshToken newToken)
    {
        if (IsRevoked)
            throw new InvalidOperationException("Cannot rotate token for a revoked session.");

        var oldToken = CurrentRefreshToken;

        if (oldToken == null)
        {
            RevokeAll();
            throw new SecurityException("Potential token reuse detected!");
        }

        oldToken.MarkAsUsed(newToken.Id);

        _refreshTokens.Add(newToken);
    }

    public void RevokeAll()
    {
        IsRevoked = true;
        foreach (var token in _refreshTokens)
        {
            token.Revoke();
        }
    }
}
