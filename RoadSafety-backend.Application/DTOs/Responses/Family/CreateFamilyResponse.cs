namespace RoadSafety_backend.Application.DTOs.Responses.Family;

public class CreateFamilyResponse
{
    public required Guid FamilyId { get; init; }
    public required string? Name { get; init; }
    public required Guid CreatedByUserId { get; init; }
    public required DateTime CreatedAt { get; init; }
}
