using RoadSafety_backend.Domain.Aggregates.MapAggregate;

namespace RoadSafety_backend.Application.DTOs.Responses.Maps;

public sealed record MapAreaFeatureCollection(string Type, IReadOnlyCollection<MapAreaFeature> Features)
{
    public static MapAreaFeatureCollection Create(IReadOnlyCollection<MapAreaFeature> features) => new("FeatureCollection", features);
}

public sealed record MapAreaFeature(string Type, GeoJsonGeometryDto Geometry, MapAreaProperties Properties)
{
    public static MapAreaFeature Create(GeoJsonGeometryDto geometry, MapAreaProperties properties) => new("Feature", geometry, properties);
}

public sealed record MapAreaProperties(Guid Id, long? OsmId, RiskLevel Risk, string? CityId);
