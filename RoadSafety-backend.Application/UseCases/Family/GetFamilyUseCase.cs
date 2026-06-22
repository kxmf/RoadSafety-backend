using RoadSafety_backend.Application.DTOs.Requests.Family;
using RoadSafety_backend.Application.DTOs.Responses.Family;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Family;

public class GetFamilyUseCase(
    IFamilyRepository familyRepository,
    ICurrentUserAccessor userAccessor
)
{
    public async Task<Result<GetFamilyResponse>> ExecuteAsync(GetFamilyRequest request, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<GetFamilyResponse>.Failure(Error.Unauthorized("User not authenticated."));

        var familyId = new FamilyId(request.FamilyId);

        var family = await familyRepository.GetFamilyByIdAsync(familyId, cancellationToken);
        if (family is null)
            return Result<GetFamilyResponse>.Failure(Error.NotFound("Family not found."));

        var currentUserId = userAccessor.UserId;

        var isMember = family.CreatedByUserId == currentUserId
                       || family.Members.Any(m => m.UserId == currentUserId);

        if (!isMember)
            return Result<GetFamilyResponse>.Failure(Error.Forbidden("User is not a member of the family."));

        var response = new GetFamilyResponse(
            family.Id,
            family.Name,
            family.CreatedByUserId,
            family.CityId
        );

        return Result<GetFamilyResponse>.Success(response);
    }
}
