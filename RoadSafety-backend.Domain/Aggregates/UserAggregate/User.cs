using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public class User
{
    public UserId Id { get; init; }

    public string HashedPassword { get; private set; }
    public UserProfile? Profile { get; private set; }
    public UserContacts Contacts { get; private set; }

    public UserRole Role { get; private set; }

    public FamilyId? FamilyId { get; private set; }

    private User() { }

    private User(UserId id, string hashedPassword, UserContacts contacts, UserRole role)
    {
        Id = id;
        HashedPassword = hashedPassword;
        Contacts = contacts;
        Role = role;
    }

    public static User Create(UserId id, string hashedPassword, UserContacts contacts, UserRole role)
    {
        return new User(id, hashedPassword, contacts, role);
    }
}