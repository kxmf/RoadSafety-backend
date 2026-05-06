namespace RoadSafety_backend.Domain.Common;

public record Error(ErrorType Type, string Message)
{
    public static readonly Error None = new(ErrorType.Failure, string.Empty);
    public static Error Failure(string message) => new(ErrorType.Failure, message);
    public static Error NotFound(string message) => new(ErrorType.NotFound, message);
    public static Error Conflict(string message) => new(ErrorType.Conflict, message);
    public static Error Unauthorized(string message) => new(ErrorType.Unauthorized, message);
}
