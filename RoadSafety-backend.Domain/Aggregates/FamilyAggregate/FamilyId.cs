namespace RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

public sealed record FamilyId(Guid Value)
{
    public static FamilyId New() => new(Guid.NewGuid());
    public static FamilyId Empty => new(Guid.Empty);

    public static implicit operator Guid(FamilyId familyId) => familyId.Value;
    public static explicit operator FamilyId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}
