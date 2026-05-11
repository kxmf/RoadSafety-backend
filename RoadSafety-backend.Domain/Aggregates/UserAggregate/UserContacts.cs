using System.Net.Mail;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public sealed record UserContacts
{
    public MailAddress? MailAddress { get; init; }
    public PhoneNumber? PhoneNumber { get; init; }

    private UserContacts() { }
    public UserContacts(string? email = null, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException("At least one of email or phone number must be provided");
        }
        
        PhoneNumber = !string.IsNullOrWhiteSpace(phoneNumber) ? new PhoneNumber(phoneNumber) : null;
        MailAddress = !string.IsNullOrEmpty(email) ? new MailAddress(email) : null;
    }
}