using NetTopologySuite.Geometries;

namespace RoadSafety_backend.Domain.Aggregates.MapAggregate;

public class MapArea
{
    public MapAreaId Id { get; init; } = null!;
    public long? OsmId { get; init; }
    public RiskLevel Risk { get; init; }
    public Polygon Geometry { get; init; } = null!;
    public string? CityId { get; init; }

    private MapArea() { }

    private MapArea(MapAreaId id, long? osmId, RiskLevel risk, Polygon geometry, string? cityId)
    {
        Id = id;
        OsmId = osmId;
        Risk = risk;
        Geometry = geometry;
        CityId = cityId;
    }

    public static MapArea Create(MapAreaId id, long? osmId, RiskLevel risk, Polygon geometry, string? cityId)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(geometry);

        if (id.Value == Guid.Empty)
            throw new ArgumentException("Map area ID cannot be empty.", nameof(id));

        return new MapArea(id, osmId, risk, geometry, cityId);
    }
}
