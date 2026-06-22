using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.TrackingAggregate;

public class ChildStats
{
    public UserId ChildId { get; init; } = null!;
    public int TotalScore { get; private set; }
    public int Rating { get; private set; }

    private ChildStats() { }

    private ChildStats(UserId childId, int totalScore, int rating)
    {
        ChildId = childId;
        TotalScore = totalScore;
        Rating = rating;
    }

    public static ChildStats Create(UserId childId, int totalScore = 0, int rating = 0)
    {
        ArgumentNullException.ThrowIfNull(childId);

        if (childId.Value == Guid.Empty)
            throw new ArgumentException("Child ID cannot be empty.", nameof(childId));

        return new ChildStats(childId, totalScore, rating);
    }
}
