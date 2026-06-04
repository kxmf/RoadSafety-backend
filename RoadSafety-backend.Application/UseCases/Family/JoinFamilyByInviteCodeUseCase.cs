using RoadSafety_backend.Application.DTOs.Requests.Family;
using RoadSafety_backend.Application.DTOs.Responses.Family;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.InviteCodeAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Family;

public class JoinFamilyByInviteCodeUseCase(
    IInviteCodeRepository inviteCodeRepository,
    IFamilyRepository familyRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<JoinFamilyByInviteCodeResponse>> ExecuteAsync(JoinFamilyByInviteCodeRequest request, CancellationToken cancellationToken)
    {
        var userId = userAccessor.UserId;
        if (userId == null)
            return Result<JoinFamilyByInviteCodeResponse>.Failure(Error.Unauthorized("User must be authenticated to join a family."));

        var existingFamily = await familyRepository.GetFamilyByMemberUserIdAsync(userId, cancellationToken);
        if (existingFamily != null)
            return Result<JoinFamilyByInviteCodeResponse>.Failure(Error.Validation("User already belongs to a family."));

        if (string.IsNullOrWhiteSpace(request.InviteCode))
            return Result<JoinFamilyByInviteCodeResponse>.Failure(Error.Validation("Invite code is required."));

        var inviteCode = await inviteCodeRepository.GetInviteCodeByValueAsync(request.InviteCode.Trim(), cancellationToken);
        if (inviteCode == null)
            return Result<JoinFamilyByInviteCodeResponse>.Failure(Error.NotFound("Invite code not found."));

        if (inviteCode.ExpiresAt < DateTime.UtcNow)
            return Result<JoinFamilyByInviteCodeResponse>.Failure(Error.Validation("Invite code has expired."));

        if (inviteCode.IsUsed)
            return Result<JoinFamilyByInviteCodeResponse>.Failure(Error.Validation("Invite code has already been used."));

        var family = await familyRepository.GetFamilyByIdAsync(inviteCode.FamilyId, cancellationToken);
        if (family == null)
            return Result<JoinFamilyByInviteCodeResponse>.Failure(Error.NotFound("Family not found."));

        if (family.Members.Any(m => m.UserId == userId))
            return Result<JoinFamilyByInviteCodeResponse>.Failure(Error.Validation("User is already a member of the family."));

        var newMember = FamilyMember.Create(userId, inviteCode.Role);
        family.AddMember(newMember);

        inviteCode.Use();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new JoinFamilyByInviteCodeResponse(userId.Value, family.Id, inviteCode.Role.ToString());

        return Result<JoinFamilyByInviteCodeResponse>.Success(response);
    }
}
