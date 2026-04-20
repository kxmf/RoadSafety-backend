namespace RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

public sealed record FamilyId(Guid Id)
{
    public static FamilyId New() => new(Guid.NewGuid());
    public static FamilyId Empty => new(Guid.Empty);

    public static implicit operator Guid(FamilyId familyId) => familyId.Id;
    public static explicit operator FamilyId(Guid value) => new(value);

    public override string ToString() => Id.ToString();
}
