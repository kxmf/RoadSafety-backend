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
        logger.LogInformation(
            "Starting map area generation for city {CityId}. Bbox: {Bbox}, GridCellSizeMeters: {GridCellSizeMeters}, GridMarginMeters: {GridMarginMeters}, CrossingBufferMeters: {CrossingBufferMeters}, MinimumPolygonAreaSquareMeters: {MinimumPolygonAreaSquareMeters}.",
            city.CityId,
            DescribeBbox(city),
            _settings.GridCellSizeMeters,
            _settings.GridMarginMeters,
            _settings.CrossingBufferMeters,
            _settings.MinimumPolygonAreaSquareMeters);

        var (ways, nodes) = await overpassClient.GetCityDataAsync(city, cancellationToken);
        logger.LogInformation(
            "Loaded OSM data for city {CityId}. Ways: {WayCount}, CrossingNodes: {CrossingNodeCount}.",
            city.CityId,
            ways.Count,
            nodes.Count);

        var areas = GenerateAreas(city, ways, nodes);

        await mapAreaRepository.ReplaceCityAreasAsync(city.CityId, areas, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Saved generated map areas for city {CityId}. Total: {TotalCount}, Red: {RedCount}, Yellow: {YellowCount}, Green: {GreenCount}.",
            city.CityId,
            areas.Count,
            areas.Count(area => area.Risk == RiskLevel.Red),
            areas.Count(area => area.Risk == RiskLevel.Yellow),
            areas.Count(area => area.Risk == RiskLevel.Green));
    }

    private List<MapArea> GenerateAreas(MapGenerationCitySettings city, List<OsmWay> ways, List<OsmNode> nodes)
    {
        var cityBounds = WebMercatorProjection.CreateWebMercatorBbox(city);
        var roads = ways.Where(w => OsmRoadClassifier.IsRoad(w.Tags)).ToList();
        var crossings = ways.Where(w => OsmRoadClassifier.IsCrossing(w.Tags)).ToList();
        var pedestrianPaths = ways.Where(w => OsmRoadClassifier.IsPedestrianPathToKeep(w.Tags)).ToList();

        crossings.AddRange(pedestrianPaths.Where(w => OsmRoadClassifier.IsCrossing(w.Tags)));

        logger.LogInformation(
            "Classified OSM data for city {CityId}. RoadWays: {RoadWayCount}, CrossingWays: {CrossingWayCount}, PedestrianPathsToKeep: {PedestrianPathCount}, CrossingNodes: {CrossingNodeCount}.",
            city.CityId,
            roads.Count,
            crossings.Count,
            pedestrianPaths.Count,
            nodes.Count);

        var redAreas = GenerateRoadAreas(cityBounds, roads);
        var yellowAreas = GenerateCrossingAreas(cityBounds, crossings, nodes);
        logger.LogInformation(
            "Generated unsafe map areas for city {CityId}. RawRedAreas: {RawRedAreaCount}, YellowAreas: {YellowAreaCount}.",
            city.CityId,
            redAreas.Count,
            yellowAreas.Count);

        redAreas = SubtractAreas(redAreas, yellowAreas);
        var greenAreas = GenerateGreenAreas(cityBounds, redAreas.Select(area => area.Polygon).ToList(), yellowAreas);

        logger.LogInformation(
            "Generated final map area polygons for city {CityId}. RedAreas: {RedAreaCount}, YellowAreas: {YellowAreaCount}, GreenAreas: {GreenAreaCount}.",
            city.CityId,
            redAreas.Count,
            yellowAreas.Count,
            greenAreas.Count);

        return redAreas.Select(area => CreateArea(RiskLevel.Red, area.Polygon, city.CityId, area.OsmId))
            .Concat(yellowAreas.Select(p => CreateArea(RiskLevel.Yellow, p, city.CityId, null)))
            .Concat(greenAreas.Select(p => CreateArea(RiskLevel.Green, p, city.CityId, null)))
            .ToList();
    }

    private List<GeneratedPolygon> GenerateRoadAreas(Polygon cityBounds, List<OsmWay> roads)
    {
        var bufferParameters = new BufferParameters { EndCapStyle = EndCapStyle.Flat };
        var polygons = new List<GeneratedPolygon>();

        foreach (var road in roads)
        {
            var line = WebMercatorProjection.ToWebMercator(road.Geometry);
            var width = OsmRoadClassifier.GetWidthMeters(road.Tags);
            var buffered = line.Buffer(width / 2.0, bufferParameters).Intersection(cityBounds);

            polygons.AddRange(ExtractPolygons(buffered).Select(polygon => new GeneratedPolygon(polygon, road.Id)));
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

    private List<GeneratedPolygon> SubtractAreas(List<GeneratedPolygon> sourceAreas, List<Polygon> areasToSubtract)
    {
        if (sourceAreas.Count == 0 || areasToSubtract.Count == 0)
            return sourceAreas;

        var subtractGeometry = CreateGeometryCollection(UnionPolygons(areasToSubtract.Cast<Geometry>())).Union();

        return sourceAreas
            .SelectMany(source => ExtractPolygons(source.Polygon.Difference(subtractGeometry))
                .Select(polygon => new GeneratedPolygon(polygon, source.OsmId)))
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

    private static string DescribeBbox(MapGenerationCitySettings city)
    {
        return FormattableString.Invariant($"{city.MinLon},{city.MinLat},{city.MaxLon},{city.MaxLat}");
    }

    private sealed record GeneratedPolygon(Polygon Polygon, long? OsmId);
}
