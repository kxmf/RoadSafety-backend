namespace RoadSafety_backend.Application.DTOs.Requests.Auth;

public record RegisterRequest
{
    public required string Login { get; init; }

    public required string Password { get; init; }
}
