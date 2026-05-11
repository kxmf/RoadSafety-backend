using System.ComponentModel.DataAnnotations;

namespace RoadSafety_backend.Application.DTOs.Requests.Auth;

public record LogOutRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    public required string RefreshToken { get; init; }
}
