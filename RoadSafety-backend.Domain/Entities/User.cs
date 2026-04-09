using RoadSafety_backend.Domain.ValueObjects;
using RoadSafety_backend.Domain.ValueObjects.IDs;

namespace RoadSafety_backend.Domain.Entities;

public class User(UserProfile profile, UserContacts contacts, Password password)
{
    public UserId Id { get; init; } = UserId.New();
    public Password Password { get; init; } = password;
    public UserProfile Profile { get; private set; } = profile;
    public UserContacts Contacts { get; private set; } = contacts;

    public void UpdateProfile(UserProfile newProfile)
    {
        if (Profile == newProfile)
            return;

        Profile = newProfile;
    }

    public void UpdateContacts(UserContacts newContacts)
    {
        if (Contacts == newContacts)
            return;

        Contacts = newContacts;
    }
}