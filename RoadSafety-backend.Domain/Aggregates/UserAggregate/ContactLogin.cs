using System.Net.Mail;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public sealed record ContactLogin
{
    public MailAddress? MailAddress { get; init; }
    public PhoneNumber? PhoneNumber { get; init; }

    public bool IsEmail => MailAddress is not null;

    private ContactLogin() { }

    private ContactLogin(MailAddress? mailAddress, PhoneNumber? phoneNumber)
    {
        MailAddress = mailAddress;
        PhoneNumber = phoneNumber;
    }

    public static Result<ContactLogin> Create(string? rawLogin)
    {
        if (string.IsNullOrWhiteSpace(rawLogin))
            return Result<ContactLogin>.Failure(Error.Validation("Login(Phone or Email) is required"));

        var login = rawLogin.Trim();

        if (MailAddress.TryCreate(login, out var mailAddress))
            return Result<ContactLogin>.Success(new ContactLogin(mailAddress, null));

        var phoneResult = PhoneNumber.Create(login);
        if (phoneResult.IsSuccess)
            return Result<ContactLogin>.Success(new ContactLogin(null, phoneResult.Value));

        return Result<ContactLogin>.Failure(Error.Validation("Login must be a valid email address or phone number"));
    }
}
