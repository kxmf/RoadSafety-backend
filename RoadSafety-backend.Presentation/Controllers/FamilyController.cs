using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadSafety_backend.Application.DTOs.Requests.Family;
using RoadSafety_backend.Application.DTOs.Responses.Family;
using RoadSafety_backend.Application.UseCases.Family;
using RoadSafety_backend.Presentation.Extensions;

namespace RoadSafety_backend.Presentation.Controllers;

[ApiController]
[Route("api/families")]
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
        var result = await createFamilyUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

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
            return this.ToProblem(result.Error);

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
        var result = await joinFamilyByInviteCodeUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("invite-code")]
    [ProducesResponseType(typeof(CreateInviteCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateInviteCode([FromBody] CreateInviteCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await createInviteCodeUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return Ok(result.Value);
    }
}
