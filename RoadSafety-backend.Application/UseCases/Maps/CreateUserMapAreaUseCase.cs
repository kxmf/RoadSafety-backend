using RoadSafety_backend.Application.DTOs.Requests.Maps;
using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public class CreateUserMapAreaUseCase(
    IFamilyRepository familyRepository,
    IMapAreaRepository mapAreaRepository,
    IUserMapAreaRepository userMapAreaRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<UserMapAreaFeature>> ExecuteAsync(CreateUserMapAreaRequest request, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<UserMapAreaFeature>.Failure(Error.Unauthorized("User not authenticated."));

        if (request.FamilyId == Guid.Empty)
            return Result<UserMapAreaFeature>.Failure(Error.Validation("familyId is required."));

        if (request.ChildId == Guid.Empty)
            return Result<UserMapAreaFeature>.Failure(Error.Validation("childId cannot be empty."));

        if (request.BaseAreaId == Guid.Empty)
            return Result<UserMapAreaFeature>.Failure(Error.Validation("baseAreaId cannot be empty."));

        var family = await familyRepository.GetFamilyByIdAsync(new FamilyId(request.FamilyId), cancellationToken);
        if (family is null)
            return Result<UserMapAreaFeature>.Failure(Error.NotFound("Family not found."));

        var currentUserId = userAccessor.UserId;
        var currentMember = family.Members.FirstOrDefault(m => m.UserId == currentUserId);
        if (currentMember is null && family.CreatedByUserId != currentUserId)
            return Result<UserMapAreaFeature>.Failure(Error.Forbidden("User is not a member of the family."));

        if (currentMember is not null && currentMember.Role != FamilyMemberRole.Parent)
            return Result<UserMapAreaFeature>.Failure(Error.Forbidden("Only parents can create user map areas."));

        var childId = request.ChildId is null ? null : new UserId(request.ChildId.Value);
        if (childId is not null && !family.Members.Any(m => m.UserId == childId))
            return Result<UserMapAreaFeature>.Failure(Error.Validation("childId must belong to the family."));

        MapAreaId? baseAreaId = null;
        if (request.BaseAreaId is not null)
        {
            baseAreaId = new MapAreaId(request.BaseAreaId.Value);
            var baseAreaExists = await mapAreaRepository.ExistsAsync(baseAreaId, cancellationToken);
            if (!baseAreaExists)
                return Result<UserMapAreaFeature>.Failure(Error.NotFound("Base map area not found."));
        }

        var geometryResult = MapGeometryMapper.FromGeoJsonPolygon(request.Geometry);
        if (!geometryResult.IsSuccess)
            return Result<UserMapAreaFeature>.Failure(geometryResult.Error);

        var userMapArea = UserMapArea.Create(
            UserMapAreaId.New(),
            family.Id,
            childId,
            baseAreaId,
            request.Risk,
            geometryResult.Value,
            currentUserId,
            DateTimeOffset.UtcNow);

        await userMapAreaRepository.CreateAsync(userMapArea, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserMapAreaFeature>.Success(MapAreaResponseMapper.ToFeature(userMapArea));
    }
}
