using RoadSafety_backend.Application.DTOs.Responses.Notifications;
using RoadSafety_backend.Domain.Aggregates.NotificationAggregate;

namespace RoadSafety_backend.Application.UseCases.Notifications;

internal static class NotificationResponseMapper
{
    public static NotificationResponse ToResponse(Notification notification)
    {
        return new NotificationResponse(
            notification.Id,
            notification.RecipientUserId.Value,
            notification.ChildId?.Value,
            notification.Type,
            notification.Title,
            notification.Body,
            notification.Risk,
            notification.Location?.Y,
            notification.Location?.X,
            notification.CreatedAt,
            notification.ReadAt);
    }
}
