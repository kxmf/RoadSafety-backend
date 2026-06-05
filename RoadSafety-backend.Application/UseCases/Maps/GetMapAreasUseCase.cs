using RoadSafety_backend.Application.DTOs.Requests.Maps;
using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public class GetMapAreasUseCase(
    IMapAreaRepository mapAreaRepository,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<MapAreaFeatureCollection>> ExecuteAsync(GetMapAreasRequest request, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<MapAreaFeatureCollection>.Failure(Error.Unauthorized("User not authenticated."));

        var bboxResult = MapGeometryMapper.CreateBboxPolygon(request.Bbox);
        if (!bboxResult.IsSuccess)
            return Result<MapAreaFeatureCollection>.Failure(bboxResult.Error);

        var areas = await mapAreaRepository.GetIntersectingAsync(bboxResult.Value, request.CityId, cancellationToken);
        var features = areas.Select(MapAreaResponseMapper.ToFeature).ToList();

        return Result<MapAreaFeatureCollection>.Success(MapAreaFeatureCollection.Create(features));
    }
}
