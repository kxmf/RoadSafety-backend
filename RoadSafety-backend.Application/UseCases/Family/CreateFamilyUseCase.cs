using RoadSafety_backend.Application.DTOs.Requests.Family;
using RoadSafety_backend.Application.DTOs.Responses.Family;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Family;

public class CreateFamilyUseCase(
    IUnitOfWork unitOfWork,
    IFamilyRepository familyRepository,
    ICurrentUserAccessor userAccessor
    )
{
    public async Task<Result<CreateFamilyResponse>> ExecuteAsync(CreateFamilyRequest request, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<CreateFamilyResponse>.Failure(Error.Unauthorized("User not authenticated."));

        var family = Domain.Aggregates.FamilyAggregate.Family.Create(request.Name, new FamilyId(Guid.NewGuid()), userAccessor.UserId);
        var familyMember = FamilyMember.Create(userAccessor.UserId, FamilyMemberRole.Parent);
        family.Name = request.Name;
        family.AddMember(familyMember);
        await familyRepository.CreateFamilyAsync(family, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<CreateFamilyResponse>.Success(new CreateFamilyResponse(family.Id, family.CreatedByUserId));
    }
}

