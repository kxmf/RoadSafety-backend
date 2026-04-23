namespace RoadSafety_backend.Domain.Common;

public record Error(int StatusCode, string Message)
{
    public static readonly Error None = new(0, string.Empty);
}
