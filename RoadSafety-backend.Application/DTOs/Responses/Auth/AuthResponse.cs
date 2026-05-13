namespace RoadSafety_backend.Application.DTOs.Responses.Auth;

public record AuthResponse(Guid UserId, string AccessToken, DateTimeOffset AccessTokenExpiresAt, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt)
{
}
