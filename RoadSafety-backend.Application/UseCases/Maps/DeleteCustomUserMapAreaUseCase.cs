using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public class DeleteCustomUserMapAreaUseCase(
    IFamilyRepository familyRepository,
    IUserMapAreaRepository userMapAreaRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<bool>> ExecuteAsync(Guid areaId, CancellationToken cancellationToken)
    {
        if (areaId == Guid.Empty)
            return Result<bool>.Failure(Error.Validation("areaId is required."));

        var userMapArea = await userMapAreaRepository.GetByIdAsync(new UserMapAreaId(areaId), cancellationToken);
        if (userMapArea is null || !userMapArea.IsCustomArea)
            return Result<bool>.Failure(Error.NotFound("Custom map area not found."));

        var accessResult = await FamilyMapAccessValidator.ValidateParentWriteAsync(
            familyRepository,
            userAccessor,
            userMapArea.FamilyId.Value,
            userMapArea.ChildId?.Value,
            cancellationToken);
        if (!accessResult.IsSuccess)
            return Result<bool>.Failure(accessResult.Error);

        await userMapAreaRepository.DeleteAsync(userMapArea, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
