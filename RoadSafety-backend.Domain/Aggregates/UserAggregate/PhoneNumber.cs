using System.Text.RegularExpressions;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public sealed record PhoneNumber
{
    private static readonly Regex PhoneRegex = new(@"^\+[1-9]\d{1,14}$", RegexOptions.Compiled);

        public string Value { get; init; } = null!;

    private PhoneNumber() { }

        private PhoneNumber(string value)
    {
        Value = value;
    }
        
    public static PhoneNumber FromTrustedSource(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number value cannot be empty.", nameof(value));
        return new PhoneNumber(value);
    }
    
    public static Result<PhoneNumber> Create(string? rawPhoneNumber)
    {
        if (string.IsNullOrWhiteSpace(rawPhoneNumber))
            return Result<PhoneNumber>.Failure(Error.Validation("Phone number cannot be empty"));

        var cleaned = Normalize(rawPhoneNumber);

        if (!PhoneRegex.IsMatch(cleaned))
            return Result<PhoneNumber>.Failure(
                Error.Validation("Invalid phone number format. Expected E.164 (e.g. +79991234567)"));

        return Result<PhoneNumber>.Success(new PhoneNumber(cleaned));
    }

    private static string Normalize(string phone)
    {
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
    
    public override string ToString() => Value;
}
