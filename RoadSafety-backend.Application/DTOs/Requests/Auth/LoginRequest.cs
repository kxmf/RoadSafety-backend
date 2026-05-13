namespace RoadSafety_backend.Application.DTOs.Requests.Auth;

public record LoginRequest
{
    public required string Login { get; init; }

    public required string Password { get; init; }
}