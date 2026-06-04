using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Application.Interfaces;

public interface ITokenService
{
    public (string AccessToken, DateTimeOffset AccessTokenExpirationDateTime) GenerateAccessToken(User user);
    public (string PlainRefreshToken, RefreshToken RefreshToken) GenerateRefreshToken(UserId userId, SessionId sessionId);
    public string HashToken(string plainToken);
}
