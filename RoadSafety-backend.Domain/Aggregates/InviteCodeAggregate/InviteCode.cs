using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.InviteCodeAggregate;

public class InviteCode
{
    public InviteCodeId Id { get; init; }

    public InviteCodeValue Value { get; init; }
    public FamilyMemberRole Role { get; init; }

    public FamilyId FamilyId { get; init; }
    public UserId CreatedByUserId { get; init; }

    public DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset? AcceptedAt { get; private set; }

    public bool IsUsed { get; private set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsUsed && !IsExpired;

    private InviteCode() { } 

    private InviteCode(InviteCodeId id, InviteCodeValue value, FamilyMemberRole role, FamilyId familyId, UserId createdByUserId, DateTime expiresAt)
    {
        Id = id;
        Value = value;
        Role = role;
        FamilyId = familyId;
        CreatedByUserId = createdByUserId;
        ExpiresAt = expiresAt;
        AcceptedAt = null;
    }

    public static InviteCode Create(InviteCodeId id, InviteCodeValue value, FamilyMemberRole role, FamilyId familyId, UserId createdByUserId, DateTime expiresAt)
    {
        return new InviteCode(id, value, role, familyId, createdByUserId, expiresAt);
    }

    public void Use() => IsUsed = true;
}
