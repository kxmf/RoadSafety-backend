namespace RoadSafety_backend.Application.DTOs.Requests.Family;

public record CreateFamilyRequest
{
    public string? Name { get; init; }
}
