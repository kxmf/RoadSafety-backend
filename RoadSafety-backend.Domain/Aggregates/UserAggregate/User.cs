using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public class User
{
    public UserId Id { get; init; }

    public string HashedPassword { get; private set; }
    public UserProfile? Profile { get; private set; }
    public UserContacts Contacts { get; private set; }

    public FamilyMember? FamilyMember { get; private set; }

    public User(UserId id, string hashedPassword, UserProfile profile, UserContacts contacts, FamilyMember familyMember)
    {
        Id = id;
        HashedPassword = hashedPassword;
        Profile = profile;
        Contacts = contacts;
        FamilyMember = familyMember;
    }
}