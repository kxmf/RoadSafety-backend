using RoadSafety_backend.Domain.Aggregates.NotificationAggregate;

namespace RoadSafety_backend.Application.Interfaces;

public interface IPushNotificationSender
{
    Task<PushNotificationSendResult> SendAsync(
        Notification notification,
        IReadOnlyCollection<string> tokens,
        CancellationToken cancellationToken);
}

public sealed record PushNotificationSendResult(IReadOnlyCollection<string> InvalidTokens)
{
    public static readonly PushNotificationSendResult Empty = new([]);
}
