using RoadSafety_backend.Domain.ValueObjects;
using RoadSafety_backend.Domain.ValueObjects.IDs;
using System.Net.Mail;

namespace RoadSafety_backend.Application.DTOs.Responses;

public record UserResponse
{
    public UserId UserId { get; init; }

    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Patronymic { get; init; }
    public DateOnly? BirthDate { get; init; }

    public MailAddress? MailAddress { get; init; }
    public PhoneNumber? PhoneNumber { get; init; }

    public UserResponse(
        UserId userId, 
        string? firstName = null,
        string? lastName = null,
        string? patronymic = null,
        DateOnly? birthDate = null,
        MailAddress? mailAddress = null,
        PhoneNumber? phoneNumber = null)
    {
        UserId = userId;
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
        BirthDate = birthDate;
        MailAddress = mailAddress;
        PhoneNumber = phoneNumber;
    }
}
