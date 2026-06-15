namespace RoadSafety_backend.Application.DTOs.Responses.Notifications;

public sealed record NotificationsResponse(IReadOnlyCollection<NotificationResponse> Notifications);
