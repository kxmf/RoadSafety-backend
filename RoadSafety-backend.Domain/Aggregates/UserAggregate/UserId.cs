namespace RoadSafety_backend.Domain.Aggregates.UserAggregate;

public sealed record UserId(Guid Id)
{
    public static UserId New() => new(Guid.NewGuid());
    public static UserId Empty => new(Guid.Empty);

    public static implicit operator Guid(UserId userId) => userId.Id;
    public static explicit operator UserId(Guid value) => new(value);

    public override string ToString() => Id.ToString();
}
