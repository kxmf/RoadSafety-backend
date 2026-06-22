using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.NotificationAggregate;

namespace RoadSafety_backend.Application.DTOs.Responses.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    Guid RecipientUserId,
    Guid? ChildId,
    NotificationType Type,
    string Title,
    string Body,
    RiskLevel? Risk,
    double? Latitude,
    double? Longitude,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);
