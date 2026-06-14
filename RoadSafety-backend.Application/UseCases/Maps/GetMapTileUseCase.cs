using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public class GetMapTileUseCase(
    IMapAreaRepository mapAreaRepository,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<byte[]>> ExecuteAsync(string cityId, int z, int x, int y, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<byte[]>.Failure(Error.Unauthorized("User not authenticated."));

        if (string.IsNullOrWhiteSpace(cityId))
            return Result<byte[]>.Failure(Error.Validation("cityId is required."));

        if (z < 12 || z > 18)
            return Result<byte[]>.Success([]);

        if (x < 0 || y < 0)
            return Result<byte[]>.Success([]);

        var maxTile = 1 << z;
        if (x >= maxTile || y >= maxTile)
            return Result<byte[]>.Success([]);

        var tile = await mapAreaRepository.GetVectorTileAsync(cityId, z, x, y, cancellationToken);

        return Result<byte[]>.Success(tile);
    }
}
