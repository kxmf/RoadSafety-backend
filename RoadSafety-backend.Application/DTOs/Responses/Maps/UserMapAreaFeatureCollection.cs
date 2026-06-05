using RoadSafety_backend.Domain.Aggregates.MapAggregate;

namespace RoadSafety_backend.Application.DTOs.Responses.Maps;

public sealed record UserMapAreaFeatureCollection(string Type, IReadOnlyCollection<UserMapAreaFeature> Features)
{
    public static UserMapAreaFeatureCollection Create(IReadOnlyCollection<UserMapAreaFeature> features) => new("FeatureCollection", features);
}

public sealed record UserMapAreaFeature(string Type, GeoJsonGeometryDto Geometry, UserMapAreaProperties Properties)
{
    public static UserMapAreaFeature Create(GeoJsonGeometryDto geometry, UserMapAreaProperties properties) => new("Feature", geometry, properties);
}

public sealed record UserMapAreaProperties(
    Guid Id,
    Guid FamilyId,
    Guid? ChildId,
    Guid? BaseAreaId,
    RiskLevel Risk,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt);
