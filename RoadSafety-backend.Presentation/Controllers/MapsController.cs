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
    GetUserMapAreasUseCase getUserMapAreasUseCase,
    GetMapCitiesUseCase getMapCitiesUseCase,
    GetMapTileUseCase getMapTileUseCase,
    GetCityMetadataUseCase getCityMetadataUseCase,
    CreateBaseAreaOverrideUseCase createBaseAreaOverrideUseCase,
    CreateCustomUserMapAreaUseCase createCustomUserMapAreaUseCase,
    ILogger<MapsController> logger) : ControllerBase
{
    private const string VectorTileContentType = "application/vnd.mapbox-vector-tile";

    [HttpGet("cities")]
    [ProducesResponseType(typeof(MapCitiesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCities(CancellationToken cancellationToken)
    {
        logger.LogInformation("Supported map cities requested.");

        var result = await getMapCitiesUseCase.ExecuteAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Supported map cities request failed. ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        logger.LogInformation("Supported map cities returned. CityCount: {CityCount}.", result.Value.Cities.Count);

        return Ok(result.Value);
    }

    [HttpGet("cities/{cityId}/metadata")]
    [ProducesResponseType(typeof(MapCityMetadataResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCityMetadata(string cityId, CancellationToken cancellationToken)
    {
        logger.LogInformation("Map city metadata requested. CityId: {CityId}.", cityId);

        var result = await getCityMetadataUseCase.ExecuteAsync(cityId, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Map city metadata request failed. CityId: {CityId}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                cityId,
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("tiles/{cityId}/{z:int}/{x:int}/{y:int}.pbf")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, VectorTileContentType)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTile(
        string cityId,
        int z,
        int x,
        int y,
        [FromQuery] string? v,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Map tile requested. CityId: {CityId}, Z: {Z}, X: {X}, Y: {Y}, Version: {Version}.", cityId, z, x, y, v);

        var result = await getMapTileUseCase.ExecuteAsync(cityId, z, x, y, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Map tile request failed. CityId: {CityId}, Z: {Z}, X: {X}, Y: {Y}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                cityId,
                z,
                x,
                y,
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        return File(result.Value, VectorTileContentType);
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

    [HttpPost("user-areas/base-overrides")]
    [ProducesResponseType(typeof(UserMapAreaFeature), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBaseOverride(
        [FromBody] CreateBaseAreaOverrideRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Base map area override requested. FamilyId: {FamilyId}, ChildId: {ChildId}, BaseAreaKey: {BaseAreaKey}, Risk: {Risk}.",
            request.FamilyId,
            request.ChildId,
            request.BaseAreaKey,
            request.Risk);

        var result = await createBaseAreaOverrideUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Base map area override failed. FamilyId: {FamilyId}, ChildId: {ChildId}, BaseAreaKey: {BaseAreaKey}, Risk: {Risk}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                request.FamilyId,
                request.ChildId,
                request.BaseAreaKey,
                request.Risk,
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPost("user-areas/custom")]
    [ProducesResponseType(typeof(UserMapAreaFeature), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCustomArea(
        [FromBody] CreateCustomUserMapAreaRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Custom user map area creation requested. FamilyId: {FamilyId}, ChildId: {ChildId}, Risk: {Risk}, RequestedCoordinates: {RequestedCoordinates}.",
            request.FamilyId,
            request.ChildId,
            request.Risk,
            DescribeCoordinates(request.Geometry));

        var result = await createCustomUserMapAreaUseCase.ExecuteAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Custom user map area creation failed. FamilyId: {FamilyId}, ChildId: {ChildId}, Risk: {Risk}, RequestedCoordinates: {RequestedCoordinates}, ErrorType: {ErrorType}, ErrorMessage: {ErrorMessage}.",
                request.FamilyId,
                request.ChildId,
                request.Risk,
                DescribeCoordinates(request.Geometry),
                result.Error.Type,
                result.Error.Message);

            return this.ToProblem(result.Error);
        }

        logger.LogInformation(
            "Custom user map area created. Id: {Id}, FamilyId: {FamilyId}, ChildId: {ChildId}, Risk: {Risk}, ReturnedCoordinates: {ReturnedCoordinates}.",
            result.Value.Properties.Id,
            result.Value.Properties.FamilyId,
            result.Value.Properties.ChildId,
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

    private static string DescribeCoordinates(IEnumerable<GeoJsonGeometryDto?> geometries)
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
            if (geometry is null)
                continue;

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
