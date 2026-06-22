using RoadSafety_backend.Application.DTOs.Requests.Maps;
using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public class GetUserMapAreasUseCase(
    IFamilyRepository familyRepository,
    IUserMapAreaRepository userMapAreaRepository,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<UserMapAreaFeatureCollection>> ExecuteAsync(GetUserMapAreasRequest request, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<UserMapAreaFeatureCollection>.Failure(Error.Unauthorized("User not authenticated."));

        if (request.FamilyId == Guid.Empty)
            return Result<UserMapAreaFeatureCollection>.Failure(Error.Validation("familyId is required."));

        if (request.ChildId == Guid.Empty)
            return Result<UserMapAreaFeatureCollection>.Failure(Error.Validation("childId cannot be empty."));

        var family = await familyRepository.GetFamilyByIdAsync(new FamilyId(request.FamilyId), cancellationToken);
        if (family is null)
            return Result<UserMapAreaFeatureCollection>.Failure(Error.NotFound("Family not found."));

        var currentUserId = userAccessor.UserId;
        if (family.CreatedByUserId != currentUserId && !family.Members.Any(m => m.UserId == currentUserId))
            return Result<UserMapAreaFeatureCollection>.Failure(Error.Forbidden("User is not a member of the family."));

        var childId = request.ChildId is null ? null : new UserId(request.ChildId.Value);
        if (childId is not null && !family.Members.Any(m => m.UserId == childId))
            return Result<UserMapAreaFeatureCollection>.Failure(Error.Validation("childId must belong to the family."));

        var areas = await userMapAreaRepository.GetByFamilyAsync(family.Id, childId, cancellationToken);
        var features = areas.Select(MapAreaResponseMapper.ToFeature).ToList();

        return Result<UserMapAreaFeatureCollection>.Success(UserMapAreaFeatureCollection.Create(features));
    }
}
