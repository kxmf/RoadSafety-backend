using NetTopologySuite.Geometries;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.MapAggregate;

public class UserMapArea
{
    public UserMapAreaId Id { get; init; } = null!;
    public FamilyId FamilyId { get; init; } = null!;
    public UserId? ChildId { get; init; }
    public string? BaseAreaKey { get; init; }
    public RiskLevel Risk { get; private set; }
    public Polygon? Geometry { get; init; }
    public UserId CreatedByUserId { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public bool IsBaseOverride => BaseAreaKey is not null;
    public bool IsCustomArea => Geometry is not null;

    private UserMapArea() { }

    private UserMapArea(
        UserMapAreaId id,
        FamilyId familyId,
        UserId? childId,
        string? baseAreaKey,
        RiskLevel risk,
        Polygon? geometry,
        UserId createdByUserId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        Id = id;
        FamilyId = familyId;
        ChildId = childId;
        BaseAreaKey = baseAreaKey;
        Risk = risk;
        Geometry = geometry;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static UserMapArea CreateBaseOverride(
        UserMapAreaId id,
        FamilyId familyId,
        UserId? childId,
        string baseAreaKey,
        RiskLevel risk,
        UserId createdByUserId,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(familyId);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseAreaKey);
        ArgumentNullException.ThrowIfNull(createdByUserId);

        if (id.Value == Guid.Empty)
            throw new ArgumentException("User map area ID cannot be empty.", nameof(id));

        if (familyId.Value == Guid.Empty)
            throw new ArgumentException("Family ID cannot be empty.", nameof(familyId));

        if (createdByUserId.Value == Guid.Empty)
            throw new ArgumentException("Creator user ID cannot be empty.", nameof(createdByUserId));

        return new UserMapArea(id, familyId, childId, baseAreaKey, risk, null, createdByUserId, createdAt, createdAt);
    }

    public static UserMapArea CreateCustomArea(
        UserMapAreaId id,
        FamilyId familyId,
        UserId? childId,
        RiskLevel risk,
        Polygon geometry,
        UserId createdByUserId,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(familyId);
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(createdByUserId);

        if (id.Value == Guid.Empty)
            throw new ArgumentException("User map area ID cannot be empty.", nameof(id));

        if (familyId.Value == Guid.Empty)
            throw new ArgumentException("Family ID cannot be empty.", nameof(familyId));

        if (createdByUserId.Value == Guid.Empty)
            throw new ArgumentException("Creator user ID cannot be empty.", nameof(createdByUserId));

        return new UserMapArea(id, familyId, childId, null, risk, geometry, createdByUserId, createdAt, createdAt);
    }

    public void UpdateRisk(RiskLevel risk, DateTimeOffset updatedAt)
    {
        Risk = risk;
        UpdatedAt = updatedAt;
    }
}
