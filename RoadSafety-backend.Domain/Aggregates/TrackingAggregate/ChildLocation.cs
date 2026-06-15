using NetTopologySuite.Geometries;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.TrackingAggregate;

public class ChildLocation
{
    public UserId ChildId { get; init; } = null!;
    public FamilyId FamilyId { get; init; } = null!;
    public Point Location { get; private set; } = null!;
    public double? AccuracyMeters { get; private set; }
    public RiskLevel CurrentRisk { get; private set; }
    public Guid? MatchedUserAreaId { get; private set; }
    public string? MatchedBaseAreaKey { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public DateTimeOffset LastUpdatedAt { get; private set; }

    private ChildLocation() { }

    private ChildLocation(
        UserId childId,
        FamilyId familyId,
        Point location,
        double? accuracyMeters,
        RiskLevel currentRisk,
        Guid? matchedUserAreaId,
        string? matchedBaseAreaKey,
        DateTimeOffset recordedAt,
        DateTimeOffset lastUpdatedAt)
    {
        ChildId = childId;
        FamilyId = familyId;
        Location = location;
        AccuracyMeters = accuracyMeters;
        CurrentRisk = currentRisk;
        MatchedUserAreaId = matchedUserAreaId;
        MatchedBaseAreaKey = matchedBaseAreaKey;
        RecordedAt = recordedAt;
        LastUpdatedAt = lastUpdatedAt;
    }

    public static ChildLocation Create(
        UserId childId,
        FamilyId familyId,
        Point location,
        double? accuracyMeters,
        RiskLevel currentRisk,
        Guid? matchedUserAreaId,
        string? matchedBaseAreaKey,
        DateTimeOffset recordedAt,
        DateTimeOffset lastUpdatedAt)
    {
        ArgumentNullException.ThrowIfNull(childId);
        ArgumentNullException.ThrowIfNull(familyId);
        ArgumentNullException.ThrowIfNull(location);

        if (childId.Value == Guid.Empty)
            throw new ArgumentException("Child ID cannot be empty.", nameof(childId));

        if (familyId.Value == Guid.Empty)
            throw new ArgumentException("Family ID cannot be empty.", nameof(familyId));

        return new ChildLocation(
            childId,
            familyId,
            location,
            accuracyMeters,
            currentRisk,
            matchedUserAreaId,
            matchedBaseAreaKey,
            recordedAt,
            lastUpdatedAt);
    }

    public void Update(
        Point location,
        double? accuracyMeters,
        RiskLevel currentRisk,
        Guid? matchedUserAreaId,
        string? matchedBaseAreaKey,
        DateTimeOffset recordedAt,
        DateTimeOffset lastUpdatedAt)
    {
        ArgumentNullException.ThrowIfNull(location);

        Location = location;
        AccuracyMeters = accuracyMeters;
        CurrentRisk = currentRisk;
        MatchedUserAreaId = matchedUserAreaId;
        MatchedBaseAreaKey = matchedBaseAreaKey;
        RecordedAt = recordedAt;
        LastUpdatedAt = lastUpdatedAt;
    }
}
