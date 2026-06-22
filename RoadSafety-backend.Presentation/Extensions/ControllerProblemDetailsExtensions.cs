using Microsoft.AspNetCore.Mvc;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Presentation.Extensions;

public static class ControllerProblemDetailsExtensions
{
    public static IActionResult ToProblem(this ControllerBase controller, Error error)
    {
        var problem = error.Type switch
        {
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Bad Request", "https://tools.ietf.org/html/rfc9110#section-15.5.1"),
            ErrorType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized", "https://tools.ietf.org/html/rfc9110#section-15.5.2"),
            ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden", "https://tools.ietf.org/html/rfc9110#section-15.5.4"),
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found", "https://tools.ietf.org/html/rfc9110#section-15.5.5"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict", "https://tools.ietf.org/html/rfc9110#section-15.5.10"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "https://tools.ietf.org/html/rfc9110#section-15.6.1")
        };

        return controller.Problem(
            detail: error.Message,
            statusCode: problem.Item1,
            title: problem.Item2,
            type: problem.Item3);
    }
}
