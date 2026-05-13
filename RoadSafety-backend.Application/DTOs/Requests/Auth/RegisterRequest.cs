using System.ComponentModel.DataAnnotations;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Application.DTOs.Requests.Auth;

public record RegisterRequest
{
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; init; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; init; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public required string Password { get; init; }

    [Required(ErrorMessage = "Role is required")]
    public required UserRole Role { get; init; }
}
