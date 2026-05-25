namespace RoadSafety_backend.Application.DTOs.Responses.Family;

public record JoinFamilyByInviteCodeResponse(Guid userId, Guid FamilyId, string role)
{
}
