using RoadSafety_backend.Application.DTOs.Responses;
using RoadSafety_backend.Domain.Entities;
using RoadSafety_backend.Domain.Interfaces;
using RoadSafety_backend.Domain.ValueObjects;
using System.Net.Mail;

namespace RoadSafety_backend.Application.UseCases;

public sealed class UserUseCase(IUserRepository userRepository)
{
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<UserResponse> CreateUserAsync(Password password, CancellationToken cancellationToken, string? mailAddress = null, PhoneNumber? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(mailAddress) && phoneNumber == null)
            throw new ArgumentException("User must have at least one contact method: Email or Phone.");

        var userContacts = new UserContacts(mailAddress, phoneNumber);

        var userInfo = new User(new UserProfile(), userContacts, password);

        var user = await _userRepository.CreateUserAsync(userInfo, cancellationToken);

        return new UserResponse(
            user.Id,
            user.Profile.FirstName,
            user.Profile.LastName,
            user.Profile.Patronymic,
            user.Profile.BirthDate,
            user.Contacts.MailAddress,
            user.Contacts.PhoneNumber);
    }
}
