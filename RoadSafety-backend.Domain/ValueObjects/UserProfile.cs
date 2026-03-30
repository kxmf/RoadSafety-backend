namespace RoadSafety_backend.Domain.ValueObjects;

public sealed record UserProfile
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string? Patronymic { get; init; }
    public DateOnly? BirthDate { get; init; }

    public UserProfile(string firstName, string lastName, string? patronymic, DateOnly? birthDate)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));
        if (birthDate > DateOnly.FromDateTime(DateTime.Now))
            throw new ArgumentException("Birth date...", nameof(birthDate));

        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
        BirthDate = birthDate;
    }

    public UserProfile(string firstName, string lastName, DateOnly birthDate)
         : this(firstName, lastName, null, birthDate)
    {
    }

    public UserProfile(string firstName, string lastName, string patronymic)
         : this(firstName, lastName, patronymic, null)
    {
    }

    public UserProfile(string firstName, string lastName)
     : this(firstName, lastName, null, null)
    {
    }
}
