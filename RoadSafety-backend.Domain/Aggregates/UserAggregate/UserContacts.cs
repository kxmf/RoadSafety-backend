using System.Net.Mail;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public sealed record UserContacts
{
    public MailAddress? MailAddress { get; init; }
    public PhoneNumber? PhoneNumber { get; init; }

    public UserContacts(string? email = null, PhoneNumber? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(email) && phoneNumber == null)
            throw new ArgumentException("User must have at least one contact method: Email or Phone.");

        if (!string.IsNullOrWhiteSpace(email))
            MailAddress = new MailAddress(email);

        PhoneNumber = phoneNumber;
    }
}