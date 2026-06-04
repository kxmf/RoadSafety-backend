using System.ComponentModel.DataAnnotations;

namespace RoadSafety_backend.Application.DTOs.Requests.Auth;

public record LoginRequest
{
    [Required(ErrorMessage = "Login(Phone or Email) is required")]
    public required string Login { get; init; }
    
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; init; }
}