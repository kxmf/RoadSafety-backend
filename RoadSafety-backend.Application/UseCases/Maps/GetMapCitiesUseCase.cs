using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public class GetMapCitiesUseCase(
    IMapCityRepository mapCityRepository,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<MapCitiesResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<MapCitiesResponse>.Failure(Error.Unauthorized("User not authenticated."));

        var cities = await mapCityRepository.GetSupportedCitiesAsync(cancellationToken);

        return Result<MapCitiesResponse>.Success(new MapCitiesResponse(cities));
    }
}
