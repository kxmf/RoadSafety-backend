using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public class User
{
    public UserId Id { get; init; } = null!;

    public string HashedPassword { get; private set; } = null!;
    public UserProfile? Profile { get; private set; }
    public UserContacts Contacts { get; private set; } = null!;

    public FamilyId? FamilyId { get; private set; }

    private User() { }

    private User(UserId id, string hashedPassword, UserContacts contacts, UserProfile? profile)
    {
        Id = id;
        HashedPassword = hashedPassword;
        Contacts = contacts;
        Profile = profile;
    }

    public static User Create(UserId id, string hashedPassword, UserContacts contacts, UserProfile? profile = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(contacts);
        
        if (id.Value == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("Hashed password cannot be empty.", nameof(hashedPassword));

        return new User(id, hashedPassword, contacts, profile);
    }

    public void SetProfile(UserProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        Profile = profile;
    }

    public void JoinFamily(FamilyId familyId)
    {
        ArgumentNullException.ThrowIfNull(familyId);
        if (FamilyId is not null && FamilyId != familyId)
            throw new InvalidOperationException("User already belongs to another family.");

        FamilyId = familyId;
    }
}