using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;

namespace RoadSafety_backend.Infrastructure.Services.MapGeneration;

internal sealed class MapAreaGenerationService(
    OverpassMapDataClient overpassClient,
    IMapAreaRepository mapAreaRepository,
    IUnitOfWork unitOfWork,
    IOptions<MapGenerationSettings> options,
    ILogger<MapAreaGenerationService> logger)
{
    private readonly MapGenerationSettings _settings = options.Value;

    public async Task GenerateCityAsync(MapGenerationCitySettings city, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting map area generation for city {CityId}.", city.CityId);

        var (ways, nodes) = await overpassClient.GetCityDataAsync(city, cancellationToken);
        var areas = GenerateAreas(city, ways, nodes);

        await mapAreaRepository.ReplaceCityAreasAsync(city.CityId, areas, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Generated {Count} map areas for city {CityId}.", areas.Count, city.CityId);
    }

    private List<MapArea> GenerateAreas(MapGenerationCitySettings city, List<OsmWay> ways, List<OsmNode> nodes)
    {
        var cityBounds = WebMercatorProjection.CreateWebMercatorBbox(city);
        var roads = ways.Where(w => OsmRoadClassifier.IsRoad(w.Tags)).ToList();
        var crossings = ways.Where(w => OsmRoadClassifier.IsCrossing(w.Tags)).ToList();
        var pedestrianPaths = ways.Where(w => OsmRoadClassifier.IsPedestrianPathToKeep(w.Tags)).ToList();

        crossings.AddRange(pedestrianPaths.Where(w => OsmRoadClassifier.IsCrossing(w.Tags)));

        var redAreas = GenerateRoadAreas(cityBounds, roads);
        var yellowAreas = GenerateCrossingAreas(cityBounds, crossings, nodes);
        redAreas = SubtractAreas(redAreas, yellowAreas);
        var greenAreas = GenerateGreenAreas(cityBounds, redAreas, yellowAreas);

        return redAreas.Select(p => CreateArea(RiskLevel.Red, p, city.CityId, null))
            .Concat(yellowAreas.Select(p => CreateArea(RiskLevel.Yellow, p, city.CityId, null)))
            .Concat(greenAreas.Select(p => CreateArea(RiskLevel.Green, p, city.CityId, null)))
            .ToList();
    }

    private List<Polygon> GenerateRoadAreas(Polygon cityBounds, List<OsmWay> roads)
    {
        var bufferParameters = new BufferParameters { EndCapStyle = EndCapStyle.Flat };
        var polygons = new List<Polygon>();

        foreach (var road in roads)
        {
            var line = WebMercatorProjection.ToWebMercator(road.Geometry);
            var width = OsmRoadClassifier.GetWidthMeters(road.Tags);
            var buffered = line.Buffer(width / 2.0, bufferParameters).Intersection(cityBounds);

            polygons.AddRange(ExtractPolygons(buffered));
        }

        return polygons;
    }

    private List<Polygon> GenerateCrossingAreas(Polygon cityBounds, List<OsmWay> crossingWays, List<OsmNode> crossingNodes)
    {
        var geometries = new List<Geometry>();

        foreach (var crossing in crossingWays)
            geometries.Add(WebMercatorProjection.ToWebMercator(crossing.Geometry).Buffer(_settings.CrossingBufferMeters));

        foreach (var node in crossingNodes)
        {
            var coordinate = WebMercatorProjection.ToWebMercator(node.Geometry.Coordinate);
            var point = new GeometryFactory(new PrecisionModel(), 3857).CreatePoint(coordinate);
            geometries.Add(point.Buffer(_settings.CrossingBufferMeters));
        }

        return UnionPolygons(geometries)
            .Select(p => p.Intersection(cityBounds))
            .SelectMany(ExtractPolygons)
            .ToList();
    }

    private List<Polygon> GenerateGreenAreas(Polygon cityBounds, List<Polygon> redAreas, List<Polygon> yellowAreas)
    {
        var greenAreas = new List<Polygon>();
        var envelope = cityBounds.EnvelopeInternal;

        for (var x = envelope.MinX; x < envelope.MaxX; x += _settings.GridCellSizeMeters)
        {
            for (var y = envelope.MinY; y < envelope.MaxY; y += _settings.GridCellSizeMeters)
            {
                var cell = WebMercatorProjection.CreateWebMercatorRectangle(
                    x,
                    y,
                    Math.Min(x + _settings.GridCellSizeMeters, envelope.MaxX),
                    Math.Min(y + _settings.GridCellSizeMeters, envelope.MaxY));

                var workCell = WebMercatorProjection.CreateWebMercatorRectangle(
                    Math.Max(x - _settings.GridMarginMeters, envelope.MinX),
                    Math.Max(y - _settings.GridMarginMeters, envelope.MinY),
                    Math.Min(x + _settings.GridCellSizeMeters + _settings.GridMarginMeters, envelope.MaxX),
                    Math.Min(y + _settings.GridCellSizeMeters + _settings.GridMarginMeters, envelope.MaxY));

                var roadNetwork = UnionPolygons(redAreas.Where(p => p.Intersects(workCell)).Cast<Geometry>());
                var yellowNetwork = UnionPolygons(yellowAreas.Where(p => p.Intersects(workCell)).Cast<Geometry>());
                var unsafeNetwork = UnionPolygons(roadNetwork.Concat(yellowNetwork).Cast<Geometry>());

                var cityCell = cityBounds.Intersection(cell);
                var green = unsafeNetwork.Count == 0 ? cityCell : cityCell.Difference(CreateGeometryCollection(unsafeNetwork).Union());
                greenAreas.AddRange(ExtractPolygons(green));
            }
        }

        return greenAreas;
    }

    private List<Polygon> SubtractAreas(List<Polygon> sourceAreas, List<Polygon> areasToSubtract)
    {
        if (sourceAreas.Count == 0 || areasToSubtract.Count == 0)
            return sourceAreas;

        var subtractGeometry = CreateGeometryCollection(UnionPolygons(areasToSubtract.Cast<Geometry>())).Union();

        return sourceAreas
            .Select(source => source.Difference(subtractGeometry))
            .SelectMany(ExtractPolygons)
            .ToList();
    }

    private static GeometryCollection CreateGeometryCollection(IReadOnlyCollection<Polygon> polygons)
    {
        var factory = new GeometryFactory(new PrecisionModel(), 3857);
        return factory.CreateGeometryCollection(polygons.Cast<Geometry>().ToArray());
    }

    private List<Polygon> UnionPolygons(IEnumerable<Geometry> geometries)
    {
        var items = geometries.Where(g => !g.IsEmpty).ToArray();
        if (items.Length == 0)
            return [];

        var factory = new GeometryFactory(new PrecisionModel(), 3857);
        return ExtractPolygons(factory.CreateGeometryCollection(items).Union());
    }

    private List<Polygon> ExtractPolygons(Geometry geometry)
    {
        if (geometry.IsEmpty)
            return [];

        if (geometry is Polygon polygon)
            return polygon.Area >= _settings.MinimumPolygonAreaSquareMeters ? [polygon] : [];

        if (geometry is MultiPolygon multiPolygon)
            return multiPolygon.Geometries
                .OfType<Polygon>()
                .Where(p => p.Area >= _settings.MinimumPolygonAreaSquareMeters)
                .ToList();

        if (geometry is GeometryCollection collection)
            return collection.Geometries.SelectMany(ExtractPolygons).ToList();

        return [];
    }

    private static MapArea CreateArea(RiskLevel risk, Polygon webMercatorPolygon, string cityId, long? osmId)
    {
        var wgs84Polygon = WebMercatorProjection.ToWgs84Polygon(webMercatorPolygon);
        wgs84Polygon.SRID = 4326;

        return MapArea.Create(MapAreaId.New(), osmId, risk, wgs84Polygon, cityId);
    }
}
