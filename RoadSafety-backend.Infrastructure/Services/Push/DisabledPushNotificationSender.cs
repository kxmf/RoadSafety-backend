using Microsoft.Extensions.Logging;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.NotificationAggregate;

namespace RoadSafety_backend.Infrastructure.Services.Push;

public sealed class DisabledPushNotificationSender(ILogger<DisabledPushNotificationSender> logger) : IPushNotificationSender
{
    public Task<PushNotificationSendResult> SendAsync(
        Notification notification,
        IReadOnlyCollection<string> tokens,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("FCM push is disabled. Skipping notification {NotificationId}", notification.Id);
        return Task.FromResult(PushNotificationSendResult.Empty);
    }
}
