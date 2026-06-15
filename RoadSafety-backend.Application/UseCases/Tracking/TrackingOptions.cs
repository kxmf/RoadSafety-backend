namespace RoadSafety_backend.Application.UseCases.Tracking;

public sealed class TrackingOptions
{
    public int RedZoneNotificationCooldownMinutes { get; set; } = 10;
}
