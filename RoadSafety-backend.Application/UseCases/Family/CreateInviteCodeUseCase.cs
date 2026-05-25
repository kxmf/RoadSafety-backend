using System;
using System.Linq;
using RoadSafety_backend.Application.DTOs.Requests.Family;
using RoadSafety_backend.Application.DTOs.Responses.Family;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.InviteCodeAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Family;

public class CreateInviteCodeUseCase(
    IFamilyRepository familyRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IInviteCodeRepository inviteCodeRepository,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<CreateInviteCodeResponse>> ExecuteAsync(CreateInviteCodeRequest request, CancellationToken cancellationToken)
    {
        var userId = userAccessor.UserId;

        if (userId == null)
            return Result<CreateInviteCodeResponse>.Failure(Error.Unauthorized("User must be authenticated to create an invite code."));

        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);

        if (user == null)
            return Result<CreateInviteCodeResponse>.Failure(Error.NotFound("User not found."));

        var familyId = user.FamilyId;
        if (familyId == null)
            return Result<CreateInviteCodeResponse>.Failure(Error.Validation("User must belong to a family to create an invite code."));

        var family = await familyRepository.GetFamilyByIdAsync(familyId, cancellationToken);
        if (family == null)
            return Result<CreateInviteCodeResponse>.Failure(Error.NotFound("Family not found."));

        var familyMember = family.Members.FirstOrDefault(m => m.UserId == userId);

        if (familyMember == null)
            return Result<CreateInviteCodeResponse>.Failure(Error.NotFound("Family member not found."));

        if (!Enum.TryParse<FamilyMemberRole>(request.InviteCodeRole, true, out var requestedRole))
            return Result<CreateInviteCodeResponse>.Failure(Error.Validation("Invalid invite code role."));

        if (familyMember.Role != requestedRole && familyMember.Role != FamilyMemberRole.Parent)
        {
            return Result<CreateInviteCodeResponse>.Failure(Error.Unauthorized("Insufficient permissions to create invite code for the requested role."));
        }

        var inviteCodeValue = InviteCodeValue.Generate();
        var inviteCodeId = InviteCodeId.New();
        var InviteCodeExpiresAt = DateTime.UtcNow.AddDays(7);

        var inviteCode = InviteCode.Create(inviteCodeId, inviteCodeValue, requestedRole, familyId, userId, InviteCodeExpiresAt);

        await inviteCodeRepository.CreateInviteCodeAsync(inviteCode, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CreateInviteCodeResponse(inviteCodeValue.ToString());

        return Result<CreateInviteCodeResponse>.Success(response);
    }
}