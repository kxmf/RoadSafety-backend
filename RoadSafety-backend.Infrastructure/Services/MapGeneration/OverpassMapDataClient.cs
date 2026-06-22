using System.Globalization;
using System.Net;
using System.Net.Http;
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
        var bboxParts = SplitBbox(city).ToList();
        var elements = new Dictionary<string, OverpassElement>(StringComparer.Ordinal);

        logger.LogInformation(
            "Requesting Overpass map data. CityId: {CityId}, OverpassUrl: {OverpassUrl}, Bbox: {Bbox}, BboxPartCount: {BboxPartCount}.",
            city.CityId,
            _settings.OverpassUrl,
            DescribeBbox(city),
            bboxParts.Count);

        foreach (var bbox in bboxParts)
        {
            logger.LogInformation(
                "Requesting Overpass map data part. CityId: {CityId}, BboxPart: {BboxPart}.",
                city.CityId,
                DescribeBbox(bbox));

            var data = await RequestBboxAsync(city, bbox, cancellationToken);
            if (data is null)
            {
                logger.LogWarning(
                    "Overpass map data response part was empty. CityId: {CityId}, BboxPart: {BboxPart}.",
                    city.CityId,
                    DescribeBbox(bbox));
                continue;
            }

            foreach (var element in data.Elements)
            {
                if (element.Id is null)
                    continue;

                elements.TryAdd($"{element.Type}:{element.Id.Value}", element);
            }

            logger.LogInformation(
                "Overpass map data part parsed. CityId: {CityId}, BboxPart: {BboxPart}, PartElementCount: {PartElementCount}, UniqueElementCount: {UniqueElementCount}.",
                city.CityId,
                DescribeBbox(bbox),
                data.Elements.Count,
                elements.Count);

            if (_settings.OverpassRequestDelayMs > 0)
                await Task.Delay(_settings.OverpassRequestDelayMs, cancellationToken);
        }

        logger.LogInformation(
            "Overpass map data parsed. CityId: {CityId}, ElementCount: {ElementCount}.",
            city.CityId,
            elements.Count);

        var nodesById = elements.Values
            .Where(e => e.Type == "node" && e.Id is not null && e.Lat is not null && e.Lon is not null)
            .ToDictionary(
                e => e.Id!.Value,
                e => GeometryFactory.CreatePoint(new Coordinate(e.Lon!.Value, e.Lat!.Value)));

        var ways = new List<OsmWay>();
        var crossingNodes = new List<OsmNode>();

        foreach (var element in elements.Values)
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

    private async Task<OverpassResponse?> RequestBboxAsync(
        MapGenerationCitySettings city,
        OverpassBbox bbox,
        CancellationToken cancellationToken)
    {
        var maxAttempts = Math.Max(1, _settings.OverpassMaxRetries + 1);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                var query = BuildQuery(bbox);
                using var content = new FormUrlEncodedContent([new KeyValuePair<string, string>("data", query)]);
                using var response = await httpClient.PostAsync(_settings.OverpassUrl, content, cancellationToken);

                logger.LogInformation(
                    "Overpass map data part response received. CityId: {CityId}, BboxPart: {BboxPart}, StatusCode: {StatusCode}, Attempt: {Attempt}/{MaxAttempts}.",
                    city.CityId,
                    DescribeBbox(bbox),
                    (int)response.StatusCode,
                    attempt,
                    maxAttempts);

                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<OverpassResponse>(cancellationToken);

                if (!IsTransientStatusCode(response.StatusCode) || attempt == maxAttempts)
                {
                    response.EnsureSuccessStatusCode();
                    return null;
                }

                await DelayBeforeRetryAsync(city, bbox, response.StatusCode, attempt, maxAttempts, cancellationToken);
            }
            catch (HttpRequestException exception) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    exception,
                    "Overpass map data part request failed. CityId: {CityId}, BboxPart: {BboxPart}, Attempt: {Attempt}/{MaxAttempts}. Retrying.",
                    city.CityId,
                    DescribeBbox(bbox),
                    attempt,
                    maxAttempts);

                await DelayBeforeRetryAsync(city, bbox, null, attempt, maxAttempts, cancellationToken);
            }
        }

        return null;
    }

    private async Task DelayBeforeRetryAsync(
        MapGenerationCitySettings city,
        OverpassBbox bbox,
        HttpStatusCode? statusCode,
        int attempt,
        int maxAttempts,
        CancellationToken cancellationToken)
    {
        var delay = TimeSpan.FromMilliseconds(Math.Max(1000, _settings.OverpassRequestDelayMs) * Math.Pow(2, attempt));
        logger.LogWarning(
            "Overpass map data part returned transient failure. CityId: {CityId}, BboxPart: {BboxPart}, StatusCode: {StatusCode}, Attempt: {Attempt}/{MaxAttempts}, RetryDelayMs: {RetryDelayMs}.",
            city.CityId,
            DescribeBbox(bbox),
            statusCode is null ? null : (int)statusCode.Value,
            attempt,
            maxAttempts,
            delay.TotalMilliseconds);

        await Task.Delay(delay, cancellationToken);
    }

    private static bool IsTransientStatusCode(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.TooManyRequests || (int)statusCode >= 500;
    }

    private IEnumerable<OverpassBbox> SplitBbox(MapGenerationCitySettings city)
    {
        var maxSide = _settings.OverpassMaxBboxSideDegrees > 0
            ? _settings.OverpassMaxBboxSideDegrees
            : double.PositiveInfinity;

        var lonParts = Math.Max(1, (int)Math.Ceiling((city.MaxLon - city.MinLon) / maxSide));
        var latParts = Math.Max(1, (int)Math.Ceiling((city.MaxLat - city.MinLat) / maxSide));

        for (var lonIndex = 0; lonIndex < lonParts; lonIndex++)
        {
            var minLon = city.MinLon + (city.MaxLon - city.MinLon) * lonIndex / lonParts;
            var maxLon = city.MinLon + (city.MaxLon - city.MinLon) * (lonIndex + 1) / lonParts;

            for (var latIndex = 0; latIndex < latParts; latIndex++)
            {
                var minLat = city.MinLat + (city.MaxLat - city.MinLat) * latIndex / latParts;
                var maxLat = city.MinLat + (city.MaxLat - city.MinLat) * (latIndex + 1) / latParts;

                yield return new OverpassBbox(minLon, minLat, maxLon, maxLat);
            }
        }
    }

    private static string BuildQuery(OverpassBbox bbox)
    {
        var minLat = bbox.MinLat.ToString(CultureInfo.InvariantCulture);
        var minLon = bbox.MinLon.ToString(CultureInfo.InvariantCulture);
        var maxLat = bbox.MaxLat.ToString(CultureInfo.InvariantCulture);
        var maxLon = bbox.MaxLon.ToString(CultureInfo.InvariantCulture);
        var bboxText = $"{minLat},{minLon},{maxLat},{maxLon}";

        return $$"""
            [out:json][timeout:120];
            (
              way["highway"]({{bboxText}});
              node["highway"="crossing"]({{bboxText}});
            );
            (._;>;);
            out body;
            """;
    }

    private static string DescribeBbox(MapGenerationCitySettings city)
    {
        return FormattableString.Invariant($"{city.MinLon},{city.MinLat},{city.MaxLon},{city.MaxLat}");
    }

    private static string DescribeBbox(OverpassBbox bbox)
    {
        return FormattableString.Invariant($"{bbox.MinLon},{bbox.MinLat},{bbox.MaxLon},{bbox.MaxLat}");
    }

    private sealed record OverpassBbox(double MinLon, double MinLat, double MaxLon, double MaxLat);

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
