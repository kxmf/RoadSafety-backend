namespace RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

public class Family
{
    public FamilyId Id { get; init; }

    public readonly List<FamilyMember> _members;
    public IReadOnlyCollection<FamilyMember> Members => _members.AsReadOnly();
}