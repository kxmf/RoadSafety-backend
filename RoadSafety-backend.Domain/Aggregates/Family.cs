using RoadSafety_backend.Domain.ValueObjects.IDs;

namespace RoadSafety_backend.Domain.Aggregates;

public class Family(UserId creatorId)
{
    public FamilyId Id { get; init; } = FamilyId.New();
}