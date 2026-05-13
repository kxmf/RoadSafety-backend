namespace RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

public class Family
{
    public FamilyId Id { get; init; } = null!;

    private readonly List<FamilyMember> _familyMembers = new();

    public IReadOnlyCollection<FamilyMember> FamilyMembers => _familyMembers.AsReadOnly();

    private Family() { }

    private Family(FamilyId id)
    {
        Id = id;
    }
    
    public static Family Create(FamilyId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (id.Value == Guid.Empty)
            throw new ArgumentException("Family ID cannot be empty.", nameof(id));

        return new Family(id);
    }
}