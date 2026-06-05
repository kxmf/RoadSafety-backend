using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadSafety_backend.Application.DTOs.Responses.Users;
using RoadSafety_backend.Application.UseCases.Users;
using RoadSafety_backend.Presentation.Extensions;

namespace RoadSafety_backend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(
    GetUserByContactUseCase getUserByContactUseCase,
    GetCurrentUserUseCase getCurrentUserUseCase) : ControllerBase
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
            return this.ToProblem(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await getCurrentUserUseCase.ExecuteAsync(cancellationToken);

        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return Ok(result.Value);
    }
}
