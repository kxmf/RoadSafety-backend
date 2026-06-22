namespace RoadSafety_backend.Domain.Aggregates.MapAggregate;

public class MapCityMetadata
{
    public string CityId { get; private set; } = string.Empty;
    public DateTimeOffset GenerationVersion { get; private set; }
    public double MinLon { get; private set; }
    public double MinLat { get; private set; }
    public double MaxLon { get; private set; }
    public double MaxLat { get; private set; }

    private MapCityMetadata() { }

    private MapCityMetadata(
        string cityId,
        DateTimeOffset generationVersion,
        double minLon,
        double minLat,
        double maxLon,
        double maxLat)
    {
        CityId = cityId;
        GenerationVersion = generationVersion;
        MinLon = minLon;
        MinLat = minLat;
        MaxLon = maxLon;
        MaxLat = maxLat;
    }

    public static MapCityMetadata Create(
        string cityId,
        DateTimeOffset generationVersion,
        double minLon,
        double minLat,
        double maxLon,
        double maxLat)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cityId);

        return new MapCityMetadata(cityId.Trim(), generationVersion, minLon, minLat, maxLon, maxLat);
    }

    public void Update(DateTimeOffset generationVersion, double minLon, double minLat, double maxLon, double maxLat)
    {
        GenerationVersion = generationVersion;
        MinLon = minLon;
        MinLat = minLat;
        MaxLon = maxLon;
        MaxLat = maxLat;
    }
}
