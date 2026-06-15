using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadSafety_backend.Application.DTOs.Requests.Tracking;
using RoadSafety_backend.Application.DTOs.Responses.Tracking;
using RoadSafety_backend.Application.UseCases.Tracking;
using RoadSafety_backend.Presentation.Extensions;

namespace RoadSafety_backend.Presentation.Controllers;

[ApiController]
[Route("api/tracking")]
[Authorize]
public class TrackingController(
    SubmitLocationUseCase submitLocationUseCase,
    GetChildLocationUseCase getChildLocationUseCase,
    GetChildrenLocationsUseCase getChildrenLocationsUseCase,
    GetChildStatsUseCase getChildStatsUseCase,
    ILogger<TrackingController> logger) : ControllerBase
{
    [HttpPost("location")]
    [ProducesResponseType(typeof(SubmitLocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SubmitLocation([FromBody] SubmitLocationRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Child location submit requested. ChildId: {ChildId}.", request.ChildId);

        var result = await submitLocationUseCase.ExecuteAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Child location submit failed. ChildId: {ChildId}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                request.ChildId,
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("children/{childId:guid}/location")]
    [ProducesResponseType(typeof(ChildLocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChildLocation(Guid childId, CancellationToken cancellationToken)
    {
        var result = await getChildLocationUseCase.ExecuteAsync(childId, cancellationToken);
        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("children/locations")]
    [ProducesResponseType(typeof(ChildLocationsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetChildrenLocations(CancellationToken cancellationToken)
    {
        var result = await getChildrenLocationsUseCase.ExecuteAsync(cancellationToken);
        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("children/{childId:guid}/stats")]
    [ProducesResponseType(typeof(ChildStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetChildStats(Guid childId, CancellationToken cancellationToken)
    {
        var result = await getChildStatsUseCase.ExecuteAsync(childId, cancellationToken);
        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return Ok(result.Value);
    }
}
