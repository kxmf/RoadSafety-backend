using System.Net.Mail;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public sealed record UserContacts
{
    public MailAddress? MailAddress { get; init; }
    public PhoneNumber? PhoneNumber { get; init; }

    private UserContacts() { }
    public UserContacts(string? email = null, string? phoneNumber = null)
    {
        PhoneNumber = new PhoneNumber(phoneNumber);
        MailAddress = new MailAddress(email);
    }
}