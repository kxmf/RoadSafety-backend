using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

public class FamilyMember
{
    public UserId UserId { get; init; }
    public FamilyMemberRole Role { get; init; }

    private FamilyMember() { }
}
