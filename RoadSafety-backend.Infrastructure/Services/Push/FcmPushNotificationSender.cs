using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using RoadSafety_backend.Application.Interfaces;
using DomainNotification = RoadSafety_backend.Domain.Aggregates.NotificationAggregate.Notification;

namespace RoadSafety_backend.Infrastructure.Services.Push;

public sealed class FcmPushNotificationSender(
    FirebaseMessaging firebaseMessaging,
    ILogger<FcmPushNotificationSender> logger) : IPushNotificationSender
{
    public async Task<PushNotificationSendResult> SendAsync(
        DomainNotification notification,
        IReadOnlyCollection<string> tokens,
        CancellationToken cancellationToken)
    {
        if (tokens.Count == 0)
            return PushNotificationSendResult.Empty;

        var invalidTokens = new List<string>();
        var distinctTokens = tokens.Distinct(StringComparer.Ordinal).ToList();

        foreach (var batch in distinctTokens.Chunk(500))
        {
            var message = new MulticastMessage
            {
                Tokens = batch,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = notification.Title,
                    Body = notification.Body
                },
                Data = BuildData(notification),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = "road_safety_alerts"
                    }
                }
            };

            try
            {
                var response = await firebaseMessaging.SendEachForMulticastAsync(message, cancellationToken);
                for (var index = 0; index < response.Responses.Count; index++)
                {
                    var sendResponse = response.Responses[index];
                    if (sendResponse.IsSuccess)
                        continue;

                    var token = batch[index];
                    if (IsInvalidToken(sendResponse.Exception))
                    {
                        invalidTokens.Add(token);
                        continue;
                    }

                    logger.LogWarning(
                        sendResponse.Exception,
                        "Failed to send FCM notification {NotificationId} to token",
                        notification.Id);
                }
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Failed to send FCM notification {NotificationId}", notification.Id);
            }
        }

        return new PushNotificationSendResult(invalidTokens);
    }

    private static Dictionary<string, string> BuildData(DomainNotification notification)
    {
        return new Dictionary<string, string>
        {
            ["notificationId"] = notification.Id.ToString(),
            ["type"] = notification.Type.ToString(),
            ["childId"] = notification.ChildId?.Value.ToString() ?? string.Empty,
            ["risk"] = notification.Risk?.ToString() ?? string.Empty,
            ["latitude"] = notification.Location?.Y.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            ["longitude"] = notification.Location?.X.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            ["createdAt"] = notification.CreatedAt.ToString("O")
        };
    }

    private static bool IsInvalidToken(FirebaseMessagingException exception)
    {
        return exception.MessagingErrorCode is MessagingErrorCode.Unregistered
            or MessagingErrorCode.InvalidArgument
            or MessagingErrorCode.SenderIdMismatch;
    }
}
