using RoadSafety_backend.Domain.Entities;
using RoadSafety_backend.Domain.ValueObjects.IDs;

namespace RoadSafety_backend.Domain.Aggregates;

public class Family
{
    public FamilyId Id { get; init; } = FamilyId.New();

    public readonly List<FamilyMember> _members = [];

    public IReadOnlyCollection<FamilyMember> Members => _members.AsReadOnly();

    public void AddChild(UserId childId)
    {
        _members.Add(new FamilyMember(Id, childId, Enums.UserRole.Child));
    }

    public void AddParent(UserId parentId)
    {
        _members.Add(new FamilyMember(Id, parentId, Enums.UserRole.Parent));
    }
}