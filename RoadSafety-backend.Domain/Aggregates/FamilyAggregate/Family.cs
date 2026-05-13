using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

public class Family
{
    public FamilyId Id { get; init; }

    private readonly List<FamilyMember> _familyMembers;

    public IReadOnlyCollection<FamilyMember> FamilyMembers => _familyMembers.AsReadOnly();

    private Family() { }
}