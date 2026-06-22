namespace RoadSafety_backend.Infrastructure.Services.MapGeneration;

public sealed class MapGenerationSettings
{
    public bool Enabled { get; init; }
    public int IntervalDays { get; init; } = 7;
    public bool RunOnStartup { get; init; }
    public string OverpassUrl { get; init; } = "https://overpass-api.de/api/interpreter";
    public string? OverpassContactEmail { get; init; }
    public double OverpassMaxBboxSideDegrees { get; init; } = 0.08;
    public int OverpassRequestDelayMs { get; init; } = 1500;
    public int OverpassMaxRetries { get; init; } = 3;
    public int GridCellSizeMeters { get; init; } = 1000;
    public int GridMarginMeters { get; init; } = 50;
    public double MinimumPolygonAreaSquareMeters { get; init; } = 10;
    public double CrossingBufferMeters { get; init; } = 12;
    public double RoadTurnSplitAngleDegrees { get; init; } = 35;
    public double MaxRoadSegmentLengthMeters { get; init; } = 80;
    public List<MapGenerationCitySettings> Cities { get; init; } = [];
}

public sealed class MapGenerationCitySettings
{
    public string CityId { get; init; } = string.Empty;
    public string? Name { get; init; }
    public double MinLon { get; init; }
    public double MinLat { get; init; }
    public double MaxLon { get; init; }
    public double MaxLat { get; init; }
}
