using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;

namespace RoadSafety_backend.Application.DTOs.Requests.Maps;

public sealed record CreateCustomUserMapAreaRequest(
    Guid FamilyId,
    Guid? ChildId,
    RiskLevel Risk,
    GeoJsonGeometryDto Geometry);
