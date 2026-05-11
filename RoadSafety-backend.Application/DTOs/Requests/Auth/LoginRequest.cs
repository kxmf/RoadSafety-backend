using System.ComponentModel.DataAnnotations;

namespace RoadSafety_backend.Application.DTOs.Requests.Auth;

public record LoginRequest
{
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string? Email { get; init; }
    
    [Phone(ErrorMessage =  "Invalid phone number format")]
    public string? PhoneNumber { get; init; }
    
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; init; }
}