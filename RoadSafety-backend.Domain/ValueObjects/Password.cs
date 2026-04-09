namespace RoadSafety_backend.Domain.ValueObjects;

public sealed record Password
{
    public string HashedPassword { get; init; }

    public Password(string hashedPassword)
    {
        HashedPassword = hashedPassword;
    }
}
