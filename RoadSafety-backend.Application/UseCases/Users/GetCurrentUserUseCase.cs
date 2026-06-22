using RoadSafety_backend.Application.DTOs.Responses.Users;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Users;

public class GetCurrentUserUseCase(
    ICurrentUserAccessor currentUserAccessor,
    IUserRepository userRepository,
    IFamilyRepository familyRepository)
{
    public async Task<Result<UserResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!currentUserAccessor.IsAuthenticated || currentUserAccessor.UserId is null || currentUserAccessor.UserId == UserId.Empty)
            return Result<UserResponse>.Failure(Error.Unauthorized("User is not authenticated"));

        var user = await userRepository.GetUserByIdAsync(currentUserAccessor.UserId, cancellationToken);

        if (user is null)
            return Result<UserResponse>.Failure(Error.NotFound("User not found"));

        var family = await familyRepository.GetFamilyByMemberUserIdAsync(currentUserAccessor.UserId, cancellationToken);
        var familyRole = family?.Members.FirstOrDefault(member => member.UserId == currentUserAccessor.UserId)?.Role
            ?? (family?.CreatedByUserId == currentUserAccessor.UserId ? FamilyMemberRole.Parent : null as FamilyMemberRole?);

        var response = new UserResponse(
            user.Id,
            user.Contacts.MailAddress?.ToString(),
            user.Contacts.PhoneNumber?.ToString(),
            user.Profile?.FirstName,
            user.Profile?.LastName,
            user.Profile?.Patronymic,
            user.Profile?.BirthDate,
            family?.Id.Value,
            familyRole?.ToString()
        );

        return Result<UserResponse>.Success(response);
    }
}

