using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public class GetCityMetadataUseCase(
    IMapAreaRepository mapAreaRepository,
    IMapCityRepository mapCityRepository)
{
    public async Task<Result<MapCityMetadataResponse>> ExecuteAsync(string cityId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cityId))
            return Result<MapCityMetadataResponse>.Failure(Error.Validation("cityId is required."));

        var metadata = await mapAreaRepository.GetCityMetadataAsync(cityId, cancellationToken);
        if (metadata is null)
            return Result<MapCityMetadataResponse>.Failure(Error.NotFound("City map metadata not found."));

        var configuredCity = (await mapCityRepository.GetSupportedCitiesAsync(cancellationToken))
            .FirstOrDefault(city => string.Equals(city.CityId, metadata.CityId, StringComparison.OrdinalIgnoreCase));

        var bbox = MergeBbox(metadata, configuredCity?.Bbox);

        return Result<MapCityMetadataResponse>.Success(new MapCityMetadataResponse(
            metadata.CityId,
            metadata.GenerationVersion,
            bbox));
    }

    private static MapCityBboxResponse MergeBbox(MapCityMetadata metadata, MapCityBboxResponse? configuredBbox)
    {
        if (configuredBbox is null)
            return new MapCityBboxResponse(metadata.MinLon, metadata.MinLat, metadata.MaxLon, metadata.MaxLat);

        return new MapCityBboxResponse(
            Math.Min(metadata.MinLon, configuredBbox.MinLon),
            Math.Min(metadata.MinLat, configuredBbox.MinLat),
            Math.Max(metadata.MaxLon, configuredBbox.MaxLon),
            Math.Max(metadata.MaxLat, configuredBbox.MaxLat));
    }
}
