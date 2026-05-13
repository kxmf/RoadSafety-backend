using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Infrastructure.Services.Settings;

namespace RoadSafety_backend.Infrastructure.Services;

public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public (string AccessTokenHash, DateTimeOffset AccessTokenExpirationDateTime) GenerateAccessToken(User user)
    {
        var expirationTime = DateTimeOffset.UtcNow.AddMinutes(_settings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _settings.Issuer,
            _settings.Audience,
            claims,
            expires: expirationTime.UtcDateTime,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expirationTime);
    }

    public (string PlainRefreshToken, RefreshToken RefreshToken) GenerateRefreshToken(UserId userId, SessionId sessionId)
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var plainToken = Convert.ToBase64String(randomNumber);

        var tokenHash = HashToken(plainToken);

        var refreshToken = RefreshToken.Create(
            RefreshTokenId.New(),
            tokenHash,
            userId,
            sessionId,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7));

        return (plainToken, refreshToken);
    }

    public string HashToken(string plainToken)
    {
        var bytes = Encoding.UTF8.GetBytes(plainToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
