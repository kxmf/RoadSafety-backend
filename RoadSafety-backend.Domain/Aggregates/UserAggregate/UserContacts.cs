using System.Net.Mail;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public sealed record UserContacts
{
    public MailAddress? MailAddress { get; init; }
    public PhoneNumber? PhoneNumber { get; init; }

    private UserContacts() { }

    private UserContacts(MailAddress? mail, PhoneNumber? phone)
    {
        MailAddress = mail;
        PhoneNumber = phone;
    }
    
    public static Result<UserContacts> Create(string? email = null, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phoneNumber))
            return Result<UserContacts>.Failure(
                Error.Validation("At least one of email or phone number must be provided"));

        MailAddress? mail = null;
        if (!string.IsNullOrWhiteSpace(email))
        {
            if (!MailAddress.TryCreate(email, out mail))
                return Result<UserContacts>.Failure(Error.Validation("Invalid email format"));
        }

        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(phoneNumber))
        {
            var phoneResult = PhoneNumber.Create(phoneNumber);
            if (phoneResult.IsFailure)
                return Result<UserContacts>.Failure(phoneResult.Error);
            phone = phoneResult.Value;
        }

        return Result<UserContacts>.Success(new UserContacts(mail, phone));
    }
}