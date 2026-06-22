using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.TrackingAggregate;

public class ChildRiskState
{
    public UserId ChildId { get; init; } = null!;
    public RiskLevel CurrentRisk { get; private set; }
    public DateTimeOffset? EnteredRedAt { get; private set; }
    public DateTimeOffset? LastRedNotificationAt { get; private set; }
    public DateTimeOffset LastUpdatedAt { get; private set; }

    private ChildRiskState() { }

    private ChildRiskState(UserId childId, RiskLevel currentRisk, DateTimeOffset? enteredRedAt, DateTimeOffset? lastRedNotificationAt, DateTimeOffset lastUpdatedAt)
    {
        ChildId = childId;
        CurrentRisk = currentRisk;
        EnteredRedAt = enteredRedAt;
        LastRedNotificationAt = lastRedNotificationAt;
        LastUpdatedAt = lastUpdatedAt;
    }

    public static ChildRiskState Create(UserId childId, RiskLevel currentRisk, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(childId);

        if (childId.Value == Guid.Empty)
            throw new ArgumentException("Child ID cannot be empty.", nameof(childId));

        return new ChildRiskState(
            childId,
            currentRisk,
            currentRisk == RiskLevel.Red ? now : null,
            null,
            now);
    }

    public bool ShouldCreateRedNotification(DateTimeOffset now, TimeSpan cooldown)
    {
        return CurrentRisk == RiskLevel.Red
               && (LastRedNotificationAt is null || now - LastRedNotificationAt >= cooldown);
    }

    public void ApplyRisk(RiskLevel risk, DateTimeOffset now)
    {
        if (risk == RiskLevel.Red)
        {
            if (CurrentRisk != RiskLevel.Red)
                EnteredRedAt = now;
        }
        else
        {
            EnteredRedAt = null;
            LastRedNotificationAt = null;
        }

        CurrentRisk = risk;
        LastUpdatedAt = now;
    }

    public void MarkRedNotificationCreated(DateTimeOffset now)
    {
        LastRedNotificationAt = now;
        LastUpdatedAt = now;
    }
}
