using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

namespace RoadSafety_backend.Application.DTOs.Requests;

public record RegisterRequest
{
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }

    public string Password { get; init; }

    public FamilyMemberRole Role { get; init; }
}
