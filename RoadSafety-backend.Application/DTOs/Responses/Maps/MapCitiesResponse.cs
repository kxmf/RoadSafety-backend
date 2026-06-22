namespace RoadSafety_backend.Application.DTOs.Responses.Maps;

public record MapCitiesResponse(IReadOnlyCollection<MapCityResponse> Cities);

public record MapCityResponse(
    string CityId,
    string Name,
    MapCityBboxResponse? Bbox);
