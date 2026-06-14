using System.ComponentModel.DataAnnotations;

namespace RoadSafety_backend.Application.DTOs.Requests.Family;

public record UpdateFamilyCityRequest
{
    [Required]
    public string? CityId { get; init; }
}
