using NetTopologySuite.Geometries;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.MapAggregate;

public class UserMapArea
{
    public UserMapAreaId Id { get; init; } = null!;
    public FamilyId FamilyId { get; init; } = null!;
    public UserId? ChildId { get; init; }
    public MapAreaId? BaseAreaId { get; init; }
    public RiskLevel Risk { get; init; }
    public Polygon Geometry { get; init; } = null!;
    public UserId CreatedByUserId { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }

    private UserMapArea() { }

    private UserMapArea(
        UserMapAreaId id,
        FamilyId familyId,
        UserId? childId,
        MapAreaId? baseAreaId,
        RiskLevel risk,
        Polygon geometry,
        UserId createdByUserId,
        DateTimeOffset createdAt)
    {
        Id = id;
        FamilyId = familyId;
        ChildId = childId;
        BaseAreaId = baseAreaId;
        Risk = risk;
        Geometry = geometry;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }

    public static UserMapArea Create(
        UserMapAreaId id,
        FamilyId familyId,
        UserId? childId,
        MapAreaId? baseAreaId,
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

        return new UserMapArea(id, familyId, childId, baseAreaId, risk, geometry, createdByUserId, createdAt);
    }
}
