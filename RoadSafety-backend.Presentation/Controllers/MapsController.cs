using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoadSafety_backend.Application.DTOs.Requests.Maps;
using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Application.UseCases.Maps;
using RoadSafety_backend.Presentation.Extensions;

namespace RoadSafety_backend.Presentation.Controllers;

[ApiController]
[Route("api/maps")]
[Authorize]
public class MapsController(
    GetMapAreasUseCase getMapAreasUseCase,
    GetUserMapAreasUseCase getUserMapAreasUseCase,
    CreateUserMapAreaUseCase createUserMapAreaUseCase) : ControllerBase
{
    [HttpGet("areas")]
    [ProducesResponseType(typeof(MapAreaFeatureCollection), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAreas(
        [FromQuery] string bbox,
        [FromQuery] string? cityId,
        CancellationToken cancellationToken)
    {
        var request = new GetMapAreasRequest(bbox, cityId);
        var result = await getMapAreasUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return Ok(result.Value);
    }

    [HttpGet("user-areas")]
    [ProducesResponseType(typeof(UserMapAreaFeatureCollection), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserAreas(
        [FromQuery] Guid familyId,
        [FromQuery] Guid? childId,
        CancellationToken cancellationToken)
    {
        var request = new GetUserMapAreasRequest(familyId, childId);
        var result = await getUserMapAreasUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return Ok(result.Value);
    }

    [HttpPost("user-areas")]
    [ProducesResponseType(typeof(UserMapAreaFeature), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateUserArea(
        [FromBody] CreateUserMapAreaRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createUserMapAreaUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
            return this.ToProblem(result.Error);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }
}
