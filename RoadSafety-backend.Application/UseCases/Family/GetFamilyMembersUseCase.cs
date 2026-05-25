using RoadSafety_backend.Application.DTOs.Requests.Family;
using RoadSafety_backend.Application.DTOs.Responses.Family;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Family;

public class GetFamilyMembersUseCase(
    IFamilyRepository familyRepository,
    ICurrentUserAccessor userAccessor
)
{
    public async Task<Result<GetFamilyMembersResponse>> ExecuteAsync(GetFamilyMembersRequest request, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null)
            return Result<GetFamilyMembersResponse>.Failure(Error.Unauthorized("User not authenticated."));

        var familyId = new FamilyId(request.FamilyId);

        var family = await familyRepository.GetFamilyByIdAsync(familyId, cancellationToken);
        if (family is null)
            return Result<GetFamilyMembersResponse>.Failure(Error.NotFound("Family not found."));

        var currentUserId = userAccessor.UserId;

        var isMember = family.CreatedByUserId == currentUserId
                       || family.Members.Any(m => m.UserId == currentUserId);

        if (!isMember)
            return Result<GetFamilyMembersResponse>.Failure(Error.Unauthorized("User is not a member of the family."));

        var members = family.Members
            .Select(m => new MemberDto(m.UserId.Value, m.Role.ToString()))
            .ToList();

        var response = new GetFamilyMembersResponse(members);
        return Result<GetFamilyMembersResponse>.Success(response);
    }
}