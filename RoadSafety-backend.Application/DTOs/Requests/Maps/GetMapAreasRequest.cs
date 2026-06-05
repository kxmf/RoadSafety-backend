namespace RoadSafety_backend.Application.DTOs.Requests.Maps;

public sealed record GetMapAreasRequest(string Bbox, string? CityId);
