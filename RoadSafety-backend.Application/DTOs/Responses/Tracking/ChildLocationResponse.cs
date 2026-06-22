using RoadSafety_backend.Domain.Aggregates.MapAggregate;

namespace RoadSafety_backend.Application.DTOs.Responses.Tracking;

public sealed record ChildLocationResponse(
    Guid ChildId,
    string DisplayName,
    double Latitude,
    double Longitude,
    double? AccuracyMeters,
    RiskLevel CurrentRisk,
    DateTimeOffset LastUpdatedAt);
