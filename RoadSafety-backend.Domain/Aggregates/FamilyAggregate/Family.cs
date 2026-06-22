using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

public class Family
{
    public FamilyId Id { get; init; } = null!;

    public UserId CreatedByUserId { get; init; } = null!;

    public string CityId { get; private set; } = string.Empty;

    private readonly List<FamilyMember> _members = [];

    public string? Name { get; set; }

    public IReadOnlyCollection<FamilyMember> Members => _members.AsReadOnly();

    private Family() { }

    private Family(FamilyId id, UserId createdByUserId, string cityId)
    {
        Id = id;
        CreatedByUserId = createdByUserId;
        CityId = cityId;
    }

    public static Family Create(string? name, FamilyId id, UserId createdByUserId, string cityId)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(cityId);

        if (id.Value == Guid.Empty)
            throw new ArgumentException("Family ID cannot be empty.", nameof(id));

        return new Family(id, createdByUserId, cityId.Trim())
        {
            Name = name
        };
    }

    public void UpdateCity(string cityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cityId);

        CityId = cityId.Trim();
    }

    public void AddMember(FamilyMember familyMember) => _members.Add(familyMember);
}
