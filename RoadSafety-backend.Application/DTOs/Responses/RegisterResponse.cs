namespace RoadSafety_backend.Application.DTOs.Responses;

public record RegisterResponse(Guid UserId, string AccessToken, DateTimeOffset AccessTokenExpiresAt, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt)
{
}
