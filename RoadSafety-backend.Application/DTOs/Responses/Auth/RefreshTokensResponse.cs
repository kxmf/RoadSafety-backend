namespace RoadSafety_backend.Application.DTOs.Responses.Auth;

public record RefreshTokensResponse(string AccessToken, DateTimeOffset AccessTokenExpiresAt, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt)
{
    
}
