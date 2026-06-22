namespace RoadSafety_backend.Domain.Aggregates.MapAggregate;

public sealed record MapAreaId(Guid Value)
{
    public static MapAreaId New() => new(Guid.NewGuid());
    public static MapAreaId Empty => new(Guid.Empty);

    public static implicit operator Guid(MapAreaId mapAreaId) => mapAreaId.Value;
    public static explicit operator MapAreaId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
