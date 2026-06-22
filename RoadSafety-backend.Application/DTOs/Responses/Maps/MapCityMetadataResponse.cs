namespace RoadSafety_backend.Application.DTOs.Responses.Maps;

public record MapCityMetadataResponse(
    string CityId,
    DateTimeOffset GenerationVersion,
    MapCityBboxResponse Bbox);

public record MapCityBboxResponse(
    double MinLon,
    double MinLat,
    double MaxLon,
    double MaxLat);
