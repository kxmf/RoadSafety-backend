using RoadSafety_backend.Domain.Aggregates.MapAggregate;

namespace RoadSafety_backend.Application.DTOs.Responses.Tracking;

public sealed record SubmitLocationResponse(
    Guid ChildId,
    RiskLevel CurrentRisk,
    Guid? MatchedUserAreaId,
    string? MatchedBaseAreaKey,
    DateTimeOffset LastUpdatedAt);
