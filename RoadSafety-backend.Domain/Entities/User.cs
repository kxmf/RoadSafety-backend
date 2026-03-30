using RoadSafety_backend.Domain.Enums;
using RoadSafety_backend.Domain.ValueObjects;

namespace RoadSafety_backend.Domain.Entities;

public class User
{
    public UserId Id { get; init; }
    public UserProfile UserProfile { get; private set; }
    public UserContacts Contacts { get; private set; }
    public UserRole UserRole { get; private set; }
}