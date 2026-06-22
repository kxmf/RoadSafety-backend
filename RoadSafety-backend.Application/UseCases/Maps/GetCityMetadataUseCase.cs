using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public class GetCityMetadataUseCase(IMapAreaRepository mapAreaRepository)
{
    public async Task<Result<MapCityMetadataResponse>> ExecuteAsync(string cityId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(cityId))
            return Result<MapCityMetadataResponse>.Failure(Error.Validation("cityId is required."));

        var metadata = await mapAreaRepository.GetCityMetadataAsync(cityId, cancellationToken);
        if (metadata is null)
            return Result<MapCityMetadataResponse>.Failure(Error.NotFound("City map metadata not found."));

        return Result<MapCityMetadataResponse>.Success(new MapCityMetadataResponse(
            metadata.CityId,
            metadata.GenerationVersion,
            new MapCityBboxResponse(metadata.MinLon, metadata.MinLat, metadata.MaxLon, metadata.MaxLat)));
    }
}
