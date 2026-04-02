namespace RoadSafety_backend.Domain.ValueObjects.IDs;

public sealed record FamilyMemberId(Guid Id)
{
    public static FamilyMemberId New() => new(Guid.NewGuid());
    public static FamilyMemberId Empty => new(Guid.Empty);

    public static implicit operator Guid(FamilyMemberId familyMemberId) => familyMemberId.Id;

    public static explicit operator FamilyMemberId(Guid value) => new(value);

    public override string ToString() => Id.ToString();
}
