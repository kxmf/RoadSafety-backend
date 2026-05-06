namespace RoadSafety_backend.Application.DTOs.Requests.Auth;

public record LogOutRequest
{
    public required string RefreshToken { get; init; }
}
