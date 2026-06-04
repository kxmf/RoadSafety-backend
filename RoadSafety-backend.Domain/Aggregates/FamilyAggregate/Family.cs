using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

public class Family
{
    public FamilyId Id { get; init; } = null!;

    public UserId CreatedByUserId { get; init; } = null!;

    private readonly List<FamilyMember> _members = [];

    public string? Name { get; set; }

    public IReadOnlyCollection<FamilyMember> Members => _members.AsReadOnly();

    private Family() { }

    private Family(FamilyId id, UserId createdByUserId)
    {
        Id = id;
        CreatedByUserId = createdByUserId;
    }

    public static Family Create(string? name, FamilyId id, UserId createdByUserId)
    {
        ArgumentNullException.ThrowIfNull(id);
        if (id.Value == Guid.Empty)
            throw new ArgumentException("Family ID cannot be empty.", nameof(id));

        return new Family(id, createdByUserId)
        {
            Name = name
        };
    }

    public void AddMember(FamilyMember familyMember) => _members.Add(familyMember);
}
