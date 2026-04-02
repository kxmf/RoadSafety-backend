using RoadSafety_backend.Domain.Enums;
using RoadSafety_backend.Domain.ValueObjects.IDs;

namespace RoadSafety_backend.Domain.Entities;

public class FamilyMember(FamilyId familyId, UserId userId, UserRole userRole)
{
    public FamilyMemberId Id { get; init; } = FamilyMemberId.New();
    public FamilyId FamilyId { get; init; } = familyId;
    public UserId UserId { get; init; } = userId;
    public UserRole UserRole { get; init; } = userRole;
}