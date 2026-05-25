using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadSafety_backend.Application.DTOs.Requests.Family;
using RoadSafety_backend.Application.DTOs.Responses.Family;
using RoadSafety_backend.Application.UseCases.Family;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FamilyController(
    CreateFamilyUseCase createFamilyUseCase,
    GetFamilyMembersUseCase getFamilyMembersUseCase,
    JoinFamilyByInviteCodeUseCase joinFamilyByInviteCodeUseCase,
    CreateInviteCodeUseCase createInviteCodeUseCase
    ) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateFamilyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateFamily([FromBody] CreateFamilyRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await createFamilyUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status401Unauthorized),
                ErrorType.Validation => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest),
                _ => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet("{familyId}/members")]
    [ProducesResponseType(typeof(GetFamilyMembersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFamilyMembers(Guid familyId, CancellationToken cancellationToken)
    {
        var request = new GetFamilyMembersRequest(familyId);
        var result = await getFamilyMembersUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status401Unauthorized),
                ErrorType.NotFound => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status404NotFound),
                _ => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("join-by-invite")]
    [ProducesResponseType(typeof(JoinFamilyByInviteCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> JoinFamilyByInviteCode([FromBody] JoinFamilyByInviteCodeRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await joinFamilyByInviteCodeUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status401Unauthorized),
                ErrorType.NotFound => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status404NotFound),
                ErrorType.Validation => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest),
                ErrorType.Conflict => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status409Conflict),
                _ => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("invite-code")]
    [ProducesResponseType(typeof(CreateInviteCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateInviteCode([FromBody] CreateInviteCodeRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await createInviteCodeUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status401Unauthorized),
                ErrorType.NotFound => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status404NotFound),
                ErrorType.Validation => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status400BadRequest),
                _ => Problem(detail: result.Error.Message, statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        return Ok(result.Value);
    }
}
