namespace RoadSafety_backend.Domain.Aggregates.InviteCodeAggregate;

public sealed record InviteCodeId(Guid Value)
{
    public static InviteCodeId New() => new(Guid.NewGuid());
    public static InviteCodeId Empty => new(Guid.Empty);

    public static implicit operator Guid(InviteCodeId inviteCodeId) => inviteCodeId.Value;
    public static explicit operator InviteCodeId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
