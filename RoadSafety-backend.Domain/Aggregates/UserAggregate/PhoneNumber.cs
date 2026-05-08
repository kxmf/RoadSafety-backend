using System.Text.RegularExpressions;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public sealed record PhoneNumber
{
    private static readonly Regex PhoneRegex = new(@"^\+[1-9]\d{1,14}$", RegexOptions.Compiled);

    public string? Value { get; init; }

    private PhoneNumber() { }
    public PhoneNumber(string? rawPhoneNumber)
    {
        var cleaned = Normalize(rawPhoneNumber);

        if (!PhoneRegex.IsMatch(cleaned))
        {
            throw new ArgumentException("Invalid phone number format. Expected E.164 (e.g. +79991234567)");
        }

        Value = cleaned;
    }

    private static string Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;

        var digitsOnly = Regex.Replace(phone, @"[^\d+]", "");

        if (digitsOnly.StartsWith("8") && digitsOnly.Length == 11)
        {
            digitsOnly = "+7" + digitsOnly.Substring(1);
        }

        else if (!digitsOnly.StartsWith("+"))
        {
            digitsOnly = "+" + digitsOnly;
        }

        return digitsOnly;
    }

    public override string ToString() => Value!;
}
