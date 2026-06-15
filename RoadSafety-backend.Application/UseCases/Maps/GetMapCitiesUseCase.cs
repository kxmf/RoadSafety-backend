using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public class GetMapCitiesUseCase(IMapCityRepository mapCityRepository)
{
    public async Task<Result<MapCitiesResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var cities = await mapCityRepository.GetSupportedCitiesAsync(cancellationToken);

        return Result<MapCitiesResponse>.Success(new MapCitiesResponse(cities));
    }
}
