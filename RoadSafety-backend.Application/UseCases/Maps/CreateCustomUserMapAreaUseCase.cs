using RoadSafety_backend.Application.DTOs.Requests.Maps;
using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public class CreateCustomUserMapAreaUseCase(
    IFamilyRepository familyRepository,
    IUserMapAreaRepository userMapAreaRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<UserMapAreaFeature>> ExecuteAsync(CreateCustomUserMapAreaRequest request, CancellationToken cancellationToken)
    {
        var accessResult = await FamilyMapAccessValidator.ValidateParentWriteAsync(
            familyRepository,
            userAccessor,
            request.FamilyId,
            request.ChildId,
            cancellationToken);
        if (!accessResult.IsSuccess)
            return Result<UserMapAreaFeature>.Failure(accessResult.Error);

        var geometryResult = MapGeometryMapper.FromGeoJsonPolygon(request.Geometry);
        if (!geometryResult.IsSuccess)
            return Result<UserMapAreaFeature>.Failure(geometryResult.Error);

        var family = accessResult.Value.Family;
        var childId = accessResult.Value.ChildId;
        var now = DateTimeOffset.UtcNow;
        var userMapArea = UserMapArea.CreateCustomArea(
            UserMapAreaId.New(),
            family.Id,
            childId,
            request.Risk,
            geometryResult.Value,
            accessResult.Value.CurrentUserId,
            now);

        await userMapAreaRepository.CreateAsync(userMapArea, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserMapAreaFeature>.Success(MapAreaResponseMapper.ToFeature(userMapArea));
    }
}
