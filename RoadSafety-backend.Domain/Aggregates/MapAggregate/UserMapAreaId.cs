namespace RoadSafety_backend.Domain.Aggregates.MapAggregate;

public sealed record UserMapAreaId(Guid Value)
{
    public static UserMapAreaId New() => new(Guid.NewGuid());
    public static UserMapAreaId Empty => new(Guid.Empty);

    public static implicit operator Guid(UserMapAreaId userMapAreaId) => userMapAreaId.Value;
    public static explicit operator UserMapAreaId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
