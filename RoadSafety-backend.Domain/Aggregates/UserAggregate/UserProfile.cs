using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public sealed record UserProfile
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Patronymic { get; init; }
    public DateOnly? BirthDate { get; init; }
    
    private UserProfile() { }

    private UserProfile(string? firstName, string? lastName, string? patronymic, DateOnly? birthDate)
    {
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
        BirthDate = birthDate;
    }
    
    public static Result<UserProfile> Create(
        string? firstName = null,
        string? lastName = null,
        string? patronymic = null,
        DateOnly? birthDate = null)
    {
        if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            return Result<UserProfile>.Failure(Error.Validation("Birth date cannot be in the future"));

        return Result<UserProfile>.Success(new UserProfile(firstName, lastName, patronymic, birthDate));
    }
}