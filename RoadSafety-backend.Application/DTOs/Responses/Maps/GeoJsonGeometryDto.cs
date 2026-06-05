namespace RoadSafety_backend.Application.DTOs.Responses.Maps;

public sealed record GeoJsonGeometryDto(string Type, double[][][] Coordinates);
