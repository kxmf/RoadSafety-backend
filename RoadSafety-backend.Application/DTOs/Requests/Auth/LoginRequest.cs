namespace RoadSafety_backend.Application.DTOs.Requests.Auth;

public record LoginRequest
{
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }

    public required string Password { get; init; }
}