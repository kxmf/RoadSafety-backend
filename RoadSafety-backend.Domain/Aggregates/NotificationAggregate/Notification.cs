using NetTopologySuite.Geometries;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.NotificationAggregate;

public class Notification
{
    public Guid Id { get; init; }
    public UserId RecipientUserId { get; init; } = null!;
    public UserId? ChildId { get; init; }
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public RiskLevel? Risk { get; init; }
    public Point? Location { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ReadAt { get; private set; }

    private Notification() { }

    private Notification(
        Guid id,
        UserId recipientUserId,
        UserId? childId,
        NotificationType type,
        string title,
        string body,
        RiskLevel? risk,
        Point? location,
        DateTimeOffset createdAt)
    {
        Id = id;
        RecipientUserId = recipientUserId;
        ChildId = childId;
        Type = type;
        Title = title;
        Body = body;
        Risk = risk;
        Location = location;
        CreatedAt = createdAt;
    }

    public static Notification CreateChildEnteredRedZone(
        Guid id,
        UserId recipientUserId,
        UserId childId,
        string childDisplayName,
        Point location,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(recipientUserId);
        ArgumentNullException.ThrowIfNull(childId);
        ArgumentNullException.ThrowIfNull(location);

        if (id == Guid.Empty)
            throw new ArgumentException("Notification ID cannot be empty.", nameof(id));

        var name = string.IsNullOrWhiteSpace(childDisplayName) ? "Child" : childDisplayName.Trim();
        return new Notification(
            id,
            recipientUserId,
            childId,
            NotificationType.ChildEnteredRedZone,
            "Child entered red zone",
            $"{name} is in a red risk zone.",
            RiskLevel.Red,
            location,
            createdAt);
    }

    public void MarkRead(DateTimeOffset readAt)
    {
        ReadAt ??= readAt;
    }
}
