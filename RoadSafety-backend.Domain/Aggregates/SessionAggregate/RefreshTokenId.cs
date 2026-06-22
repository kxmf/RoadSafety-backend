namespace RoadSafety_backend.Domain.Aggregates.SessionAggregate;

public sealed record RefreshTokenId(Guid Value)
{
    public static RefreshTokenId New() => new(Guid.NewGuid());
    public static RefreshTokenId Empty => new(Guid.Empty);

    public static implicit operator Guid(RefreshTokenId refreshTokenId) => refreshTokenId.Value;
    public static explicit operator RefreshTokenId(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}

