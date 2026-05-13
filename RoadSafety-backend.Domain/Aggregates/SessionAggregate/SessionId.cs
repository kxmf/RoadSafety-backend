namespace RoadSafety_backend.Domain.Aggregates.SessionAggregate;

public sealed record SessionId(Guid Id)
{
    public static SessionId New() => new(Guid.NewGuid());
    public static SessionId Empty => new(Guid.Empty);

    public static implicit operator Guid(SessionId sessionId) => sessionId.Id;
    public static explicit operator SessionId(Guid value) => new(value);

    public override string ToString() => Id.ToString();
}
