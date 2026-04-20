using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

public class FamilyMember
{
    public FamilyId FamilyId { get; init; }
    public Family Family { get; init; }

    public UserId UserId { get; init; }
    public User User { get; init; }

    public FamilyMemberRole UserRole { get; private set; }
}