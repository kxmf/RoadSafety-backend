using System.Net.Mail;

namespace RoadSafety_backend.Domain.ValueObjects;

public sealed record UserContacts
{
    public MailAddress? MailAddress { get; init; }
    public PhoneNumber? PhoneNumber { get; init; }

    public UserContacts(string? mailAddress, PhoneNumber? phoneNumber)
    {
        if (mailAddress != null)
            MailAddress = new MailAddress(mailAddress);

        PhoneNumber = phoneNumber;
    }

    public UserContacts(string mailAddress)
         : this(mailAddress, null)
    {
    }

    public UserContacts(PhoneNumber phoneNumber)
         : this(null, phoneNumber)
    {
    }
}
