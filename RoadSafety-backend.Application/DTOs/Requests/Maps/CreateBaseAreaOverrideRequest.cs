using RoadSafety_backend.Domain.Aggregates.MapAggregate;

namespace RoadSafety_backend.Application.DTOs.Requests.Maps;

public sealed record CreateBaseAreaOverrideRequest(
    Guid FamilyId,
    Guid? ChildId,
    string BaseAreaKey,
    RiskLevel Risk);
