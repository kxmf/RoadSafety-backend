namespace RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

public class FamilyCode
{
    public int Code { get; init; }

    public FamilyId FamilyId { get; init; }

    public FamilyMemberRole UserRole { get; init; }
}
