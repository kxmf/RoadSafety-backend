namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public sealed record UserProfile
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Patronymic { get; init; }
    public DateOnly? BirthDate { get; init; }

    public UserProfile(string? firstName = null, string? lastName = null, string? patronymic = null, DateOnly? birthDate = null)
    {
        if (birthDate > DateOnly.FromDateTime(DateTime.Now))
            throw new ArgumentException("Birth date cannot be in the future", nameof(birthDate));

        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
        BirthDate = birthDate;
    }
}