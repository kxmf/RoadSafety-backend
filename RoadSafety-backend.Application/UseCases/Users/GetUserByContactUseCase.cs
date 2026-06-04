using RoadSafety_backend.Application.DTOs.Responses.Users;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Users;

public class GetUserByContactUseCase(
    IUserRepository userRepository,
    IFamilyRepository familyRepository)
{
    public async Task<Result<UserResponse>> ExecuteAsync(string? email, string? phone, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
            return Result<UserResponse>.Failure(Error.Validation("Either email or phone must be provided"));

        User? user = null;

        if (!string.IsNullOrWhiteSpace(email))
            user = await userRepository.GetUserByEmailAsync(email, cancellationToken);
        else if (!string.IsNullOrWhiteSpace(phone))
            user = await userRepository.GetUserByPhoneAsync(phone, cancellationToken);

        if (user is null)
            return Result<UserResponse>.Failure(Error.NotFound("User not found"));

        var family = await familyRepository.GetFamilyByMemberUserIdAsync(user.Id, cancellationToken);

        var response = new UserResponse(
            user.Id,
            user.Contacts.MailAddress?.ToString(),
            user.Contacts.PhoneNumber?.ToString(),
            user.Profile?.FirstName,
            user.Profile?.LastName,
            user.Profile?.Patronymic,
            user.Profile?.BirthDate,
            family?.Id.Value
        );

        return Result<UserResponse>.Success(response);
    }
}
