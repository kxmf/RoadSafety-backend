using NetTopologySuite.Geometries;

namespace RoadSafety_backend.Domain.Aggregates.MapAggregate;

public interface IMapAreaRepository
{
    Task<List<MapArea>> GetIntersectingAsync(Polygon bbox, string? cityId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(MapAreaId id, CancellationToken cancellationToken);
    Task ReplaceCityAreasAsync(string cityId, IReadOnlyCollection<MapArea> areas, CancellationToken cancellationToken);
}
