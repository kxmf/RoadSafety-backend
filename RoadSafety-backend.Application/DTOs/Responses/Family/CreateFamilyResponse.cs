namespace RoadSafety_backend.Application.DTOs.Responses.Family;

public record CreateFamilyResponse(Guid FamilyId, Guid CreatedByUserId)
{
}
