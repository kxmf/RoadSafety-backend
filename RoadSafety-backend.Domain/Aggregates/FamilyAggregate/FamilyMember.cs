using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

public class FamilyMember
{
    public UserId UserId { get; init; }
    public FamilyMemberRole Role { get; init; }

    private FamilyMember() { }

    private FamilyMember(UserId userId, FamilyMemberRole role)
    {
        UserId = userId;
        Role = role;
    }

    public static FamilyMember Create(UserId userId, FamilyMemberRole role)
    {
        return new FamilyMember(userId, role);
    }
}
