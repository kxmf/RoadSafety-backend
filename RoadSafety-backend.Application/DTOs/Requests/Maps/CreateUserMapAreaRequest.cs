using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;

namespace RoadSafety_backend.Application.DTOs.Requests.Maps;

public sealed record CreateUserMapAreaRequest(
    Guid FamilyId,
    Guid? ChildId,
    Guid? BaseAreaId,
    RiskLevel Risk,
    GeoJsonGeometryDto Geometry);
