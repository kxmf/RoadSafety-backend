using RoadSafety_backend.Application.DTOs.Requests.Maps;
using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public class CreateBaseAreaOverrideUseCase(
    IFamilyRepository familyRepository,
    IMapAreaRepository mapAreaRepository,
    IUserMapAreaRepository userMapAreaRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<UserMapAreaFeature>> ExecuteAsync(CreateBaseAreaOverrideRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.BaseAreaKey))
            return Result<UserMapAreaFeature>.Failure(Error.Validation("baseAreaKey is required."));

        var accessResult = await FamilyMapAccessValidator.ValidateParentWriteAsync(
            familyRepository,
            userAccessor,
            request.FamilyId,
            request.ChildId,
            cancellationToken);
        if (!accessResult.IsSuccess)
            return Result<UserMapAreaFeature>.Failure(accessResult.Error);

        var baseAreaExists = await mapAreaRepository.ExistsByBaseAreaKeyAsync(request.BaseAreaKey, cancellationToken);
        if (!baseAreaExists)
            return Result<UserMapAreaFeature>.Failure(Error.NotFound("Base map area not found."));

        var family = accessResult.Value.Family;
        var childId = accessResult.Value.ChildId;
        var currentUserId = accessResult.Value.CurrentUserId;
        var now = DateTimeOffset.UtcNow;

        var mapOverride = await userMapAreaRepository.GetBaseOverrideAsync(family.Id, childId, request.BaseAreaKey, cancellationToken);
        if (mapOverride is null)
        {
            mapOverride = UserMapArea.CreateBaseOverride(
                UserMapAreaId.New(),
                family.Id,
                childId,
                request.BaseAreaKey,
                request.Risk,
                currentUserId,
                now);

            await userMapAreaRepository.CreateAsync(mapOverride, cancellationToken);
        }
        else
        {
            mapOverride.UpdateRisk(request.Risk, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserMapAreaFeature>.Success(MapAreaResponseMapper.ToFeature(mapOverride));
    }
}
