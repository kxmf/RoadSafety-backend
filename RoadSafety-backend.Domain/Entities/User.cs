using RoadSafety_backend.Domain.ValueObjects;
using RoadSafety_backend.Domain.ValueObjects.IDs;

namespace RoadSafety_backend.Domain.Entities;

public class User(UserProfile profile, UserContacts contacts)
{
    public UserId Id { get; init; } = UserId.New();
    public UserProfile Profile { get; private set; } = profile;
    public UserContacts Contacts { get; private set; } = contacts;
}