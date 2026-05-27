using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadSafety_backend.Application.DTOs.Responses.Users;
using RoadSafety_backend.Application.UseCases.Users;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(GetUserByContactUseCase getUserByContactUseCase) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByContact(
        [FromQuery] string? email,
        [FromQuery] string? phone,
        CancellationToken cancellationToken)
    {
        var result = await getUserByContactUseCase.ExecuteAsync(email, phone, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.Validation => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest),
                ErrorType.NotFound => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status404NotFound),
                _ => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.Value);
    }
}
