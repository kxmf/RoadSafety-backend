using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public class DeleteBaseAreaOverrideUseCase(
    IFamilyRepository familyRepository,
    IUserMapAreaRepository userMapAreaRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<bool>> ExecuteAsync(Guid familyId, Guid? childId, string baseAreaKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(baseAreaKey))
            return Result<bool>.Failure(Error.Validation("baseAreaKey is required."));

        var accessResult = await FamilyMapAccessValidator.ValidateParentWriteAsync(
            familyRepository,
            userAccessor,
            familyId,
            childId,
            cancellationToken);
        if (!accessResult.IsSuccess)
            return Result<bool>.Failure(accessResult.Error);

        await userMapAreaRepository.DeleteBaseOverrideAsync(
            accessResult.Value.Family.Id,
            accessResult.Value.ChildId,
            baseAreaKey,
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
