namespace RoadSafety_backend.Application.DTOs.Responses.Maps;

public sealed record AlertZonesResponse(string CityId, DateTimeOffset? GenerationVersion, IReadOnlyCollection<AlertZoneResponse> Zones);

public sealed record AlertZoneResponse(
    Guid Id,
    string? BaseAreaKey,
    string Risk,
    string Source,
    GeoJsonGeometryDto Geometry);
