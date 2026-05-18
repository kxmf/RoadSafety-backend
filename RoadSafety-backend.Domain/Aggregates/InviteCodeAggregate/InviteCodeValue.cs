using SimpleBase;

namespace RoadSafety_backend.Domain.Aggregates.InviteCodeAggregate;

public readonly record struct InviteCodeValue
{
    public string Value { get; init; }

    private InviteCodeValue(string value) => Value = value;

    public static InviteCodeValue Generate()
    {
        var bytes = new byte[4];
        Random.Shared.NextBytes(bytes);
        return new InviteCodeValue(Base58.Bitcoin.Encode(bytes));
    }

    public static InviteCodeValue Create(string value)
    {
        return new InviteCodeValue(value);
    }

    public static bool TryParse(string? input, out InviteCodeValue result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        if (input.Length != 6) return false;

        try
        {
            _ = Base58.Bitcoin.Decode(input);

            result = new InviteCodeValue(input);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public override string ToString() => Value;

    public static implicit operator string(InviteCodeValue code) => code.Value;
}