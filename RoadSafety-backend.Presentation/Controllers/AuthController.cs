using Microsoft.AspNetCore.Mvc;
using RoadSafety_backend.Application.DTOs.Requests.Auth;
using RoadSafety_backend.Application.DTOs.Responses.Auth;
using RoadSafety_backend.Application.UseCases.Auth;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    LoginUseCase loginUseCase,
    LogOutUseCase logOutUseCase,
    RefreshTokensUseCase refreshTokensUseCase,
    RegisterUseCase registerUseCase
    ) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegisterUser([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await registerUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.Validation => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest),
                ErrorType.Conflict => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status409Conflict),
                _ => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginUser([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await loginUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status401Unauthorized),
                _ => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokensResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshUserTokens([FromBody] RefreshTokensRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await refreshTokensUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status401Unauthorized),
                _ => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogOutUser([FromBody] LogOutRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await logOutUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status401Unauthorized),
                _ => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        return NoContent();
    }
}