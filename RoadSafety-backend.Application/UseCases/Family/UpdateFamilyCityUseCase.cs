using RoadSafety_backend.Application.DTOs.Requests.Family;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Family;

public class UpdateFamilyCityUseCase(
    IUnitOfWork unitOfWork,
    IFamilyRepository familyRepository,
    IUserMapAreaRepository userMapAreaRepository,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<bool>> ExecuteAsync(Guid familyId, UpdateFamilyCityRequest request, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<bool>.Failure(Error.Unauthorized("User not authenticated."));

        if (familyId == Guid.Empty)
            return Result<bool>.Failure(Error.Validation("familyId is required."));

        if (string.IsNullOrWhiteSpace(request.CityId))
            return Result<bool>.Failure(Error.Validation("cityId is required."));

        var family = await familyRepository.GetFamilyByIdAsync(new FamilyId(familyId), cancellationToken);
        if (family is null)
            return Result<bool>.Failure(Error.NotFound("Family not found."));

        var currentUserId = userAccessor.UserId;
        var currentMember = family.Members.FirstOrDefault(m => m.UserId == currentUserId);
        if (family.CreatedByUserId != currentUserId && currentMember is null)
            return Result<bool>.Failure(Error.Forbidden("User is not a member of the family."));

        if (currentMember is not null && currentMember.Role != FamilyMemberRole.Parent)
            return Result<bool>.Failure(Error.Forbidden("Only parents can change family city."));

        family.UpdateCity(request.CityId);
        await userMapAreaRepository.DeleteFamilyAreasAsync(family.Id, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
