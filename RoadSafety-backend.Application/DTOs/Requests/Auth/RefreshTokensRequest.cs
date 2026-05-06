namespace RoadSafety_backend.Application.DTOs.Requests.Auth;

public record RefreshTokensRequest
{
    public required string RefreshToken { get; init; }
}
