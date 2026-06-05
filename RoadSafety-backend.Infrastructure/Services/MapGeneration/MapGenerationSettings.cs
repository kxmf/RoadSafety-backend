namespace RoadSafety_backend.Infrastructure.Services.MapGeneration;

public sealed class MapGenerationSettings
{
    public bool Enabled { get; init; }
    public int IntervalDays { get; init; } = 7;
    public bool RunOnStartup { get; init; }
    public string OverpassUrl { get; init; } = "https://overpass-api.de/api/interpreter";
    public string? OverpassContactEmail { get; init; }
    public int GridCellSizeMeters { get; init; } = 1000;
    public int GridMarginMeters { get; init; } = 50;
    public double MinimumPolygonAreaSquareMeters { get; init; } = 10;
    public double CrossingBufferMeters { get; init; } = 6;
    public List<MapGenerationCitySettings> Cities { get; init; } = [];
}

public sealed class MapGenerationCitySettings
{
    public string CityId { get; init; } = string.Empty;
    public double MinLon { get; init; }
    public double MinLat { get; init; }
    public double MaxLon { get; init; }
    public double MaxLat { get; init; }
}
