using System.Text.Json.Serialization;

namespace RoadSafety_backend.Domain.Aggregates.NotificationAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationType
{
    ChildEnteredRedZone
}
