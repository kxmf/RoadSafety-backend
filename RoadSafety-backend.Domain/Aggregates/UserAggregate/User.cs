using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public class User
{
    public UserId Id { get; init; }

    public Password Password { get; init; }
    public UserProfile Profile { get; private set; }
    public UserContacts Contacts { get; private set; }

    public FamilyMember FamilyMember { get; private set; }
}