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
    CreateUserMapAreaUseCase createUserMapAreaUseCase,
    ILogger<MapsController> logger) : ControllerBase
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
        logger.LogInformation("Map areas requested. Bbox: {Bbox}, CityId: {CityId}.", bbox, cityId);

        var request = new GetMapAreasRequest(bbox, cityId);
        var result = await getMapAreasUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Map areas request failed. Bbox: {Bbox}, CityId: {CityId}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                bbox,
                cityId,
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        logger.LogInformation(
            "Map areas returned. Bbox: {Bbox}, CityId: {CityId}, FeatureCount: {FeatureCount}, ReturnedCoordinates: {ReturnedCoordinates}.",
            bbox,
            cityId,
            result.Value.Features.Count,
            DescribeCoordinates(result.Value.Features.Select(feature => feature.Geometry)));

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
        logger.LogInformation("User map areas requested. FamilyId: {FamilyId}, ChildId: {ChildId}.", familyId, childId);

        var request = new GetUserMapAreasRequest(familyId, childId);
        var result = await getUserMapAreasUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "User map areas request failed. FamilyId: {FamilyId}, ChildId: {ChildId}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                familyId,
                childId,
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        logger.LogInformation(
            "User map areas returned. FamilyId: {FamilyId}, ChildId: {ChildId}, FeatureCount: {FeatureCount}, ReturnedCoordinates: {ReturnedCoordinates}.",
            familyId,
            childId,
            result.Value.Features.Count,
            DescribeCoordinates(result.Value.Features.Select(feature => feature.Geometry)));

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
        logger.LogInformation(
            "User map area creation requested. FamilyId: {FamilyId}, ChildId: {ChildId}, BaseAreaId: {BaseAreaId}, Risk: {Risk}, RequestedCoordinates: {RequestedCoordinates}.",
            request.FamilyId,
            request.ChildId,
            request.BaseAreaId,
            request.Risk,
            DescribeCoordinates(request.Geometry));

        var result = await createUserMapAreaUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "User map area creation failed. FamilyId: {FamilyId}, ChildId: {ChildId}, BaseAreaId: {BaseAreaId}, Risk: {Risk}, RequestedCoordinates: {RequestedCoordinates}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                request.FamilyId,
                request.ChildId,
                request.BaseAreaId,
                request.Risk,
                DescribeCoordinates(request.Geometry),
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        logger.LogInformation(
            "User map area created. Id: {Id}, FamilyId: {FamilyId}, ChildId: {ChildId}, BaseAreaId: {BaseAreaId}, Risk: {Risk}, ReturnedCoordinates: {ReturnedCoordinates}.",
            result.Value.Properties.Id,
            result.Value.Properties.FamilyId,
            result.Value.Properties.ChildId,
            result.Value.Properties.BaseAreaId,
            result.Value.Properties.Risk,
            DescribeCoordinates(result.Value.Geometry));

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    private static string DescribeCoordinates(GeoJsonGeometryDto? geometry)
    {
        return geometry is null
            ? "none"
            : DescribeCoordinates([geometry]);
    }

    private static string DescribeCoordinates(IEnumerable<GeoJsonGeometryDto> geometries)
    {
        var geometryCount = 0;
        var ringCount = 0;
        var coordinateCount = 0;
        var minLon = double.PositiveInfinity;
        var minLat = double.PositiveInfinity;
        var maxLon = double.NegativeInfinity;
        var maxLat = double.NegativeInfinity;

        foreach (var geometry in geometries)
        {
            geometryCount++;

            foreach (var ring in geometry.Coordinates)
            {
                ringCount++;

                foreach (var coordinate in ring)
                {
                    if (coordinate.Length < 2)
                        continue;

                    var lon = coordinate[0];
                    var lat = coordinate[1];

                    minLon = Math.Min(minLon, lon);
                    minLat = Math.Min(minLat, lat);
                    maxLon = Math.Max(maxLon, lon);
                    maxLat = Math.Max(maxLat, lat);
                    coordinateCount++;
                }
            }
        }

        if (coordinateCount == 0)
            return $"geometries={geometryCount}, rings={ringCount}, coordinates=0, bbox=empty";

        return FormattableString.Invariant(
            $"geometries={geometryCount}, rings={ringCount}, coordinates={coordinateCount}, bbox={minLon},{minLat},{maxLon},{maxLat}");
    }
}
