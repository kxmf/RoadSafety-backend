namespace RoadSafety_backend.Domain.Aggregates.MapAggregate;

public interface IMapAreaRepository
{
    Task<byte[]> GetVectorTileAsync(string cityId, int z, int x, int y, CancellationToken cancellationToken);
    Task<MapCityMetadata?> GetCityMetadataAsync(string cityId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(MapAreaId id, CancellationToken cancellationToken);
    Task<bool> ExistsByBaseAreaKeyAsync(string baseAreaKey, CancellationToken cancellationToken);
    Task ReplaceCityAreasAsync(string cityId, IReadOnlyCollection<MapArea> areas, CancellationToken cancellationToken);
    Task UpsertCityMetadataAsync(
        string cityId,
        DateTimeOffset generationVersion,
        double minLon,
        double minLat,
        double maxLon,
        double maxLat,
        CancellationToken cancellationToken);
}
