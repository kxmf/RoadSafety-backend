using RoadSafety_backend.Domain.Enums;
using RoadSafety_backend.Domain.ValueObjects.IDs;

namespace RoadSafety_backend.Domain.ValueObjects;

public class FamilyCode(FamilyId familyId, UserRole userRole)
{
    public int Code { get; init; } = Random.Shared.Next(100_000, 1_000_000);

    public FamilyId FamilyId { get; init; } = familyId;

    public UserRole UserRole { get; init; } = userRole;
}
