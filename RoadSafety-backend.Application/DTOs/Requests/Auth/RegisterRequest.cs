using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Application.DTOs.Requests.Auth;

public record RegisterRequest
{
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }

    public required string Password { get; init; }

    public required UserRole Role { get; init; }
}
