using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;

namespace RoadSafety_backend.Infrastructure.Services.MapGeneration;

internal sealed class OverpassMapDataClient(
    HttpClient httpClient,
    IOptions<MapGenerationSettings> options,
    ILogger<OverpassMapDataClient> logger)
{
    private readonly MapGenerationSettings _settings = options.Value;
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), 4326);

    public async Task<(List<OsmWay> Ways, List<OsmNode> Nodes)> GetCityDataAsync(
        MapGenerationCitySettings city,
        CancellationToken cancellationToken)
    {
        var query = BuildQuery(city);
        using var content = new FormUrlEncodedContent([new KeyValuePair<string, string>("data", query)]);

        logger.LogInformation(
            "Requesting Overpass map data. CityId: {CityId}, OverpassUrl: {OverpassUrl}, Bbox: {Bbox}.",
            city.CityId,
            _settings.OverpassUrl,
            DescribeBbox(city));

        var response = await httpClient.PostAsync(_settings.OverpassUrl, content, cancellationToken);
        logger.LogInformation(
            "Overpass map data response received. CityId: {CityId}, StatusCode: {StatusCode}.",
            city.CityId,
            (int)response.StatusCode);

        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<OverpassResponse>(cancellationToken);
        if (data is null)
        {
            logger.LogWarning("Overpass map data response was empty. CityId: {CityId}.", city.CityId);
            return ([], []);
        }

        logger.LogInformation(
            "Overpass map data parsed. CityId: {CityId}, ElementCount: {ElementCount}.",
            city.CityId,
            data.Elements.Count);

        var nodesById = data.Elements
            .Where(e => e.Type == "node" && e.Id is not null && e.Lat is not null && e.Lon is not null)
            .ToDictionary(
                e => e.Id!.Value,
                e => GeometryFactory.CreatePoint(new Coordinate(e.Lon!.Value, e.Lat!.Value)));

        var ways = new List<OsmWay>();
        var crossingNodes = new List<OsmNode>();

        foreach (var element in data.Elements)
        {
            var tags = element.Tags ?? new Dictionary<string, string>();

            if (element.Type == "node" && element.Id is not null && nodesById.TryGetValue(element.Id.Value, out var point) && OsmRoadClassifier.IsCrossing(tags))
            {
                crossingNodes.Add(new OsmNode(element.Id.Value, tags, point));
                continue;
            }

            if (element.Type != "way" || element.Id is null || element.Nodes is null || element.Nodes.Length < 2)
                continue;

            var coordinates = element.Nodes
                .Where(nodesById.ContainsKey)
                .Select(nodeId => nodesById[nodeId].Coordinate)
                .ToArray();

            if (coordinates.Length < 2)
                continue;

            ways.Add(new OsmWay(element.Id.Value, tags, GeometryFactory.CreateLineString(coordinates)));
        }

        logger.LogInformation(
            "Overpass map data converted. CityId: {CityId}, Ways: {WayCount}, CrossingNodes: {CrossingNodeCount}.",
            city.CityId,
            ways.Count,
            crossingNodes.Count);

        return (ways, crossingNodes);
    }

    private static string BuildQuery(MapGenerationCitySettings city)
    {
        var minLat = city.MinLat.ToString(CultureInfo.InvariantCulture);
        var minLon = city.MinLon.ToString(CultureInfo.InvariantCulture);
        var maxLat = city.MaxLat.ToString(CultureInfo.InvariantCulture);
        var maxLon = city.MaxLon.ToString(CultureInfo.InvariantCulture);
        var bbox = $"{minLat},{minLon},{maxLat},{maxLon}";

        return $$"""
            [out:json][timeout:120];
            (
              way["highway"]({{bbox}});
              node["highway"="crossing"]({{bbox}});
            );
            (._;>;);
            out body;
            """;
    }

    private static string DescribeBbox(MapGenerationCitySettings city)
    {
        return FormattableString.Invariant($"{city.MinLon},{city.MinLat},{city.MaxLon},{city.MaxLat}");
    }

    private sealed class OverpassResponse
    {
        [JsonPropertyName("elements")]
        public List<OverpassElement> Elements { get; init; } = [];
    }

    private sealed class OverpassElement
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = string.Empty;

        [JsonPropertyName("id")]
        public long? Id { get; init; }

        [JsonPropertyName("lat")]
        public double? Lat { get; init; }

        [JsonPropertyName("lon")]
        public double? Lon { get; init; }

        [JsonPropertyName("nodes")]
        public long[]? Nodes { get; init; }

        [JsonPropertyName("tags")]
        public Dictionary<string, string>? Tags { get; init; }
    }
}
