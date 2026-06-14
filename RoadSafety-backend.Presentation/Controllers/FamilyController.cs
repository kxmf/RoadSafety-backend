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
    CreateInviteCodeUseCase createInviteCodeUseCase,
    UpdateFamilyCityUseCase updateFamilyCityUseCase,
    ILogger<FamilyController> logger
    ) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(CreateFamilyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateFamily([FromBody] CreateFamilyRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Family creation requested. NameProvided: {NameProvided}.", !string.IsNullOrWhiteSpace(request.Name));

        var result = await createFamilyUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Family creation failed. NameProvided: {NameProvided}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                !string.IsNullOrWhiteSpace(request.Name),
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        logger.LogInformation(
            "Family created. FamilyId: {FamilyId}, CreatedByUserId: {CreatedByUserId}.",
            result.Value.FamilyId,
            result.Value.CreatedByUserId);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPut("{familyId}/city")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFamilyCity(
        Guid familyId,
        [FromBody] UpdateFamilyCityRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Family city update requested. FamilyId: {FamilyId}, CityId: {CityId}.", familyId, request.CityId);

        var result = await updateFamilyCityUseCase.ExecuteAsync(familyId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Family city update failed. FamilyId: {FamilyId}, CityId: {CityId}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                familyId,
                request.CityId,
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        logger.LogInformation("Family city updated. FamilyId: {FamilyId}, CityId: {CityId}.", familyId, request.CityId);

        return NoContent();
    }

    [HttpGet("{familyId}/members")]
    [ProducesResponseType(typeof(GetFamilyMembersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFamilyMembers(Guid familyId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Family members requested. FamilyId: {FamilyId}.", familyId);

        var request = new GetFamilyMembersRequest(familyId);
        var result = await getFamilyMembersUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Family members request failed. FamilyId: {FamilyId}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                familyId,
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        logger.LogInformation(
            "Family members returned. FamilyId: {FamilyId}, MemberCount: {MemberCount}.",
            familyId,
            result.Value.Members.Count());

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
        logger.LogInformation(
            "Join family by invite requested. UserRole: {UserRole}, InviteCodeLength: {InviteCodeLength}.",
            request.UserRole,
            request.InviteCode?.Length ?? 0);

        var result = await joinFamilyByInviteCodeUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Join family by invite failed. UserRole: {UserRole}, InviteCodeLength: {InviteCodeLength}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                request.UserRole,
                request.InviteCode?.Length ?? 0,
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        logger.LogInformation(
            "Joined family by invite. UserId: {UserId}, FamilyId: {FamilyId}, Role: {Role}.",
            result.Value.userId,
            result.Value.FamilyId,
            result.Value.role);

        return Ok(result.Value);
    }

    [HttpPost("invite-code")]
    [ProducesResponseType(typeof(CreateInviteCodeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateInviteCode([FromBody] CreateInviteCodeRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Invite code creation requested. InviteCodeRole: {InviteCodeRole}.", request.InviteCodeRole);

        var result = await createInviteCodeUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Invite code creation failed. InviteCodeRole: {InviteCodeRole}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                request.InviteCodeRole,
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        logger.LogInformation(
            "Invite code created. InviteCodeRole: {InviteCodeRole}, InviteCodeLength: {InviteCodeLength}.",
            request.InviteCodeRole,
            result.Value.InviteCode.Length);

        return Ok(result.Value);
    }
}
