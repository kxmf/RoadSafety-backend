using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Operation.Buffer;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.OverlayNG;
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

        logger.LogInformation(
            "Saving generated map areas for city {CityId}. Total: {TotalCount}, Red: {RedCount}, Yellow: {YellowCount}, Green: {GreenCount}.",
            city.CityId,
            areas.Count,
            areas.Count(area => area.Risk == RiskLevel.Red),
            areas.Count(area => area.Risk == RiskLevel.Yellow),
            areas.Count(area => area.Risk == RiskLevel.Green));

        await mapAreaRepository.ReplaceCityAreasAsync(city.CityId, areas, cancellationToken);
        await mapAreaRepository.UpsertCityMetadataAsync(
            city.CityId,
            DateTimeOffset.UtcNow,
            city.MinLon,
            city.MinLat,
            city.MaxLon,
            city.MaxLat,
            cancellationToken);

        var savedChanges = await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Saved generated map areas for city {CityId}. SavedChanges: {SavedChanges}.",
            city.CityId,
            savedChanges);
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

        var roadSegments = GenerateRoadSegments(roads);
        var redAreas = GenerateRoadAreas(cityBounds, roadSegments);
        var yellowAreas = GenerateCrossingAreas(cityBounds, crossings, nodes);
        logger.LogInformation(
            "Generated unsafe map areas for city {CityId}. RoadSegments: {RoadSegmentCount}, RawRedAreas: {RawRedAreaCount}, YellowAreas: {YellowAreaCount}.",
            city.CityId,
            roadSegments.Count,
            redAreas.Count,
            yellowAreas.Count);

        var redSubtractStopwatch = Stopwatch.StartNew();
        redAreas = SubtractAreas(redAreas, yellowAreas);
        logger.LogInformation(
            "Subtracted crossing areas from red map areas for city {CityId}. RedAreas: {RedAreaCount}, ElapsedMs: {ElapsedMs}.",
            city.CityId,
            redAreas.Count,
            redSubtractStopwatch.ElapsedMilliseconds);

        var greenAreas = GenerateGreenAreas(
            city.CityId,
            cityBounds,
            roadSegments,
            redAreas.Select(area => area.Polygon).ToList(),
            yellowAreas);

        logger.LogInformation(
            "Generated final map area polygons for city {CityId}. RedAreas: {RedAreaCount}, YellowAreas: {YellowAreaCount}, GreenAreas: {GreenAreaCount}.",
            city.CityId,
            redAreas.Count,
            yellowAreas.Count,
            greenAreas.Count);

        var areas = redAreas.Select(area => CreateArea(RiskLevel.Red, area.Polygon, city.CityId, area.OsmId))
            .Concat(yellowAreas.Select(p => CreateArea(RiskLevel.Yellow, p, city.CityId, null)))
            .Concat(greenAreas.Select(p => CreateArea(RiskLevel.Green, p, city.CityId, null)))
            .ToList();

        return DeduplicateAreasByBaseKey(city.CityId, areas);
    }

    private List<RoadSegment> GenerateRoadSegments(List<OsmWay> roads)
    {
        return roads
            .AsParallel()
            .AsOrdered()
            .SelectMany(road =>
            {
                var line = WebMercatorProjection.ToWebMercator(road.Geometry);
                var width = OsmRoadClassifier.GetWidthMeters(road.Tags);

                return SplitRoadLine(line)
                    .Select(segment => new RoadSegment(segment, road.Id, width));
            })
            .ToList();
    }

    private List<GeneratedPolygon> GenerateRoadAreas(Polygon cityBounds, List<RoadSegment> roadSegments)
    {
        return roadSegments
            .AsParallel()
            .AsOrdered()
            .SelectMany(segment =>
            {
                var bufferParameters = new BufferParameters { EndCapStyle = EndCapStyle.Round };
                var buffered = IntersectAreas(segment.Geometry.Buffer(segment.WidthMeters / 2.0, bufferParameters), cityBounds);
                return ExtractPolygons(buffered).Select(polygon => new GeneratedPolygon(polygon, segment.OsmId));
            })
            .ToList();
    }

    private List<LineString> SplitRoadLine(LineString line)
    {
        if (line.NumPoints < 2)
            return [];

        var coordinates = line.Coordinates;
        var segments = new List<LineString>();
        var current = new List<Coordinate> { coordinates[0].Copy() };
        var currentLength = 0.0;
        var maxSegmentLength = _settings.MaxRoadSegmentLengthMeters > 0
            ? _settings.MaxRoadSegmentLengthMeters
            : double.PositiveInfinity;

        for (var i = 1; i < coordinates.Length; i++)
        {
            var previous = coordinates[i - 1];
            var currentCoordinate = coordinates[i];
            var stepLength = previous.Distance(currentCoordinate);

            if (stepLength == 0)
                continue;

            var consumedLength = 0.0;
            while (currentLength + stepLength - consumedLength > maxSegmentLength)
            {
                var splitDistance = maxSegmentLength - currentLength;
                if (splitDistance <= 0)
                {
                    AddLineSegment(segments, line.Factory, current);
                    current = [current[^1].Copy()];
                    currentLength = 0.0;
                    continue;
                }

                consumedLength += splitDistance;

                var splitCoordinate = Interpolate(previous, currentCoordinate, consumedLength / stepLength);
                current.Add(splitCoordinate);
                AddLineSegment(segments, line.Factory, current);

                current = [splitCoordinate.Copy()];
                currentLength = 0.0;
            }

            current.Add(currentCoordinate.Copy());
            currentLength += stepLength - consumedLength;

            if (i < coordinates.Length - 1 && IsRoadTurn(previous, currentCoordinate, coordinates[i + 1]))
            {
                AddLineSegment(segments, line.Factory, current);
                current = [currentCoordinate.Copy()];
                currentLength = 0.0;
            }
        }

        AddLineSegment(segments, line.Factory, current);
        return segments;
    }

    private static Coordinate Interpolate(Coordinate start, Coordinate end, double fraction)
    {
        return new Coordinate(
            start.X + (end.X - start.X) * fraction,
            start.Y + (end.Y - start.Y) * fraction);
    }

    private bool IsRoadTurn(Coordinate previous, Coordinate current, Coordinate next)
    {
        var firstX = current.X - previous.X;
        var firstY = current.Y - previous.Y;
        var secondX = next.X - current.X;
        var secondY = next.Y - current.Y;
        var firstLength = Math.Sqrt(firstX * firstX + firstY * firstY);
        var secondLength = Math.Sqrt(secondX * secondX + secondY * secondY);

        if (firstLength == 0 || secondLength == 0)
            return false;

        var dot = firstX * secondX + firstY * secondY;
        var cosine = Math.Clamp(dot / (firstLength * secondLength), -1.0, 1.0);
        var angleDegrees = Math.Acos(cosine) * 180.0 / Math.PI;

        return angleDegrees >= _settings.RoadTurnSplitAngleDegrees;
    }

    private static void AddLineSegment(List<LineString> segments, GeometryFactory factory, IReadOnlyList<Coordinate> coordinates)
    {
        if (coordinates.Count >= 2 && coordinates[0].Distance(coordinates[^1]) > 0)
            segments.Add(factory.CreateLineString(coordinates.Select(coordinate => coordinate.Copy()).ToArray()));
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
            .Select(p => IntersectAreas(p, cityBounds))
            .SelectMany(ExtractPolygons)
            .ToList();
    }

    private List<Polygon> GenerateGreenAreas(
        string cityId,
        Polygon cityBounds,
        List<RoadSegment> roadSegments,
        List<Polygon> redAreas,
        List<Polygon> yellowAreas)
    {
        var stopwatch = Stopwatch.StartNew();
        var unsafeAreas = redAreas.Concat(yellowAreas).Cast<Geometry>().Where(area => !area.IsEmpty).ToArray();
        var unsafeIndex = BuildSpatialIndex(unsafeAreas.OfType<Polygon>());

        logger.LogInformation(
            "Starting green map area generation for city {CityId}. RoadSegments: {RoadSegmentCount}, UnsafeAreaCount: {UnsafeAreaCount}, GreenRoadSideBandMeters: {GreenRoadSideBandMeters}.",
            cityId,
            roadSegments.Count,
            unsafeAreas.Length,
            _settings.GreenRoadSideBandMeters);

        var greenAreas = roadSegments
            .AsParallel()
            .AsOrdered()
            .SelectMany(roadSegment => CreateGreenSideBands(roadSegment)
                .SelectMany(sideBand => ClipGreenBand(sideBand, cityBounds, unsafeIndex)))
            .ToList();

        logger.LogInformation(
            "Finished green map area generation for city {CityId}. ProcessedRoadSegments: {ProcessedRoadSegments}/{RoadSegmentCount}, GreenAreas: {GreenAreaCount}, ElapsedMs: {ElapsedMs}.",
            cityId,
            roadSegments.Count,
            roadSegments.Count,
            greenAreas.Count,
            stopwatch.ElapsedMilliseconds);

        return greenAreas;
    }

    private List<Polygon> CreateGreenSideBands(RoadSegment roadSegment)
    {
        var coordinates = roadSegment.Geometry.Coordinates;
        if (coordinates.Length < 2)
            return [];

        var start = coordinates[0];
        var end = coordinates[^1];
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        var length = Math.Sqrt(dx * dx + dy * dy);

        if (length == 0)
            return [];

        var normalX = -dy / length;
        var normalY = dx / length;
        var innerDistance = roadSegment.WidthMeters / 2.0;
        var outerDistance = innerDistance + _settings.GreenRoadSideBandMeters;

        return [
            CreateSideBandPolygon(roadSegment.Geometry.Factory, start, end, normalX, normalY, innerDistance, outerDistance),
            CreateSideBandPolygon(roadSegment.Geometry.Factory, start, end, -normalX, -normalY, innerDistance, outerDistance)
        ];
    }

    private List<Polygon> ClipGreenBand(Polygon sideBand, Polygon cityBounds, STRtree<Polygon> unsafeIndex)
    {
        var insideCity = IntersectAreas(sideBand, cityBounds);
        if (insideCity.IsEmpty)
            return [];

        var nearbyUnsafeAreas = unsafeIndex
            .Query(sideBand.EnvelopeInternal)
            .Where(area => area.Intersects(sideBand))
            .Cast<Geometry>()
            .ToArray();

        var safeGeometry = nearbyUnsafeAreas.Length == 0
            ? insideCity
            : DifferenceAreas(insideCity, UnionAreaGeometry(nearbyUnsafeAreas));

        return ExtractPolygons(safeGeometry)
            .Where(IsConvexPolygon)
            .ToList();
    }

    private static STRtree<Polygon> BuildSpatialIndex(IEnumerable<Polygon> polygons)
    {
        var index = new STRtree<Polygon>();

        foreach (var polygon in polygons.Where(p => !p.IsEmpty))
            index.Insert(polygon.EnvelopeInternal, polygon);

        index.Build();
        return index;
    }

    private static Polygon CreateSideBandPolygon(
        GeometryFactory factory,
        Coordinate start,
        Coordinate end,
        double normalX,
        double normalY,
        double innerDistance,
        double outerDistance)
    {
        return factory.CreatePolygon([
            Offset(start, normalX, normalY, innerDistance),
            Offset(end, normalX, normalY, innerDistance),
            Offset(end, normalX, normalY, outerDistance),
            Offset(start, normalX, normalY, outerDistance),
            Offset(start, normalX, normalY, innerDistance)
        ]);
    }

    private static Coordinate Offset(Coordinate coordinate, double normalX, double normalY, double distance)
    {
        return new Coordinate(coordinate.X + normalX * distance, coordinate.Y + normalY * distance);
    }

    private static bool IsConvexPolygon(Polygon polygon)
    {
        var coordinates = polygon.ExteriorRing.Coordinates;
        if (polygon.NumInteriorRings > 0 || coordinates.Length < 4)
            return false;

        var sign = 0;
        for (var i = 0; i < coordinates.Length - 1; i++)
        {
            var previous = coordinates[(i + coordinates.Length - 2) % (coordinates.Length - 1)];
            var current = coordinates[i];
            var next = coordinates[(i + 1) % (coordinates.Length - 1)];
            var cross = Cross(previous, current, next);

            if (Math.Abs(cross) < 1e-9)
                continue;

            var currentSign = cross > 0 ? 1 : -1;
            if (sign == 0)
                sign = currentSign;
            else if (sign != currentSign)
                return false;
        }

        return true;
    }

    private static double Cross(Coordinate previous, Coordinate current, Coordinate next)
    {
        var firstX = current.X - previous.X;
        var firstY = current.Y - previous.Y;
        var secondX = next.X - current.X;
        var secondY = next.Y - current.Y;

        return firstX * secondY - firstY * secondX;
    }

    private List<GeneratedPolygon> SubtractAreas(List<GeneratedPolygon> sourceAreas, List<Polygon> areasToSubtract)
    {
        if (sourceAreas.Count == 0 || areasToSubtract.Count == 0)
            return sourceAreas;

        var subtractIndex = BuildSpatialIndex(areasToSubtract);

        return sourceAreas
            .AsParallel()
            .AsOrdered()
            .SelectMany(source =>
            {
                var nearbyAreas = subtractIndex
                    .Query(source.Polygon.EnvelopeInternal)
                    .Where(area => area.Intersects(source.Polygon))
                    .Cast<Geometry>()
                    .ToArray();

                var geometry = nearbyAreas.Length == 0
                    ? source.Polygon
                    : DifferenceAreas(source.Polygon, UnionAreaGeometry(nearbyAreas));

                return ExtractPolygons(geometry)
                    .Select(polygon => new GeneratedPolygon(polygon, source.OsmId));
            })
            .ToList();
    }

    private List<Polygon> UnionPolygons(IEnumerable<Geometry> geometries)
    {
        var items = geometries.Where(g => !g.IsEmpty).ToArray();
        if (items.Length == 0)
            return [];

        return ExtractPolygons(UnionGeometries(items));
    }

    private Geometry UnionAreaGeometry(IEnumerable<Geometry> geometries)
    {
        var polygons = geometries
            .SelectMany(ExtractPolygons)
            .Cast<Geometry>()
            .ToArray();

        if (polygons.Length == 0)
            return CreateEmptyAreaGeometry();

        return ExtractAreaGeometry(UnionGeometries(polygons));
    }

    private Geometry IntersectAreas(Geometry left, Geometry right)
    {
        if (left.IsEmpty || right.IsEmpty)
            return left.Factory.CreateGeometryCollection();

        try
        {
            return ExtractAreaGeometry(OverlayNGRobust.Overlay(left, right, SpatialFunction.Intersection));
        }
        catch (TopologyException)
        {
            return ExtractAreaGeometry(OverlayNGRobust.Overlay(left.Buffer(0), right.Buffer(0), SpatialFunction.Intersection));
        }
    }

    private Geometry DifferenceAreas(Geometry left, Geometry right)
    {
        var polygonalRight = ExtractAreaGeometry(right);
        if (left.IsEmpty || polygonalRight.IsEmpty)
            return ExtractAreaGeometry(left);

        try
        {
            return ExtractAreaGeometry(OverlayNGRobust.Overlay(left, polygonalRight, SpatialFunction.Difference));
        }
        catch (TopologyException)
        {
            return ExtractAreaGeometry(OverlayNGRobust.Overlay(left.Buffer(0), polygonalRight.Buffer(0), SpatialFunction.Difference));
        }
    }

    private Geometry ExtractAreaGeometry(Geometry geometry)
    {
        if (geometry.IsEmpty)
            return geometry.Factory.CreateGeometryCollection();

        var polygons = ExtractPolygons(geometry).Cast<Geometry>().ToArray();
        if (polygons.Length == 0)
            return geometry.Factory.CreateGeometryCollection();

        if (polygons.Length == 1)
            return polygons[0];

        return UnionGeometries(polygons);
    }

    private static Geometry CreateEmptyAreaGeometry()
    {
        return GeometryFactory.Default.CreateGeometryCollection();
    }

    private static Geometry UnionGeometries(IReadOnlyCollection<Geometry> geometries)
    {
        try
        {
            return OverlayNGRobust.Union(geometries);
        }
        catch (TopologyException)
        {
            return OverlayNGRobust.Union(geometries.Select(geometry => geometry.Buffer(0)).ToArray());
        }
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

        var baseAreaKey = CreateBaseAreaKey(cityId, risk, osmId, wgs84Polygon);

        return MapArea.Create(MapAreaId.New(), osmId, baseAreaKey, risk, wgs84Polygon, cityId);
    }

    private List<MapArea> DeduplicateAreasByBaseKey(string cityId, List<MapArea> areas)
    {
        var uniqueAreas = new Dictionary<string, MapArea>(StringComparer.Ordinal);

        foreach (var area in areas)
            uniqueAreas.TryAdd(area.BaseAreaKey, area);

        var duplicateCount = areas.Count - uniqueAreas.Count;
        if (duplicateCount > 0)
            logger.LogWarning(
                "Removed duplicate generated map areas for city {CityId}. Duplicates: {DuplicateCount}, UniqueAreas: {UniqueAreaCount}.",
                cityId,
                duplicateCount,
                uniqueAreas.Count);

        return uniqueAreas.Values.ToList();
    }

    private static string CreateBaseAreaKey(string cityId, RiskLevel risk, long? osmId, Polygon polygon)
    {
        var envelope = polygon.EnvelopeInternal;
        var rawKey = string.Create(
            CultureInfo.InvariantCulture,
            $"{cityId}:{risk}:{osmId}:{Math.Round(envelope.MinX, 7)}:{Math.Round(envelope.MinY, 7)}:{Math.Round(envelope.MaxX, 7)}:{Math.Round(envelope.MaxY, 7)}:{Math.Round(polygon.Area, 12)}:{polygon.AsText()}");
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey))).ToLowerInvariant()[..16];

        return $"{cityId}:{risk.ToString().ToLowerInvariant()}:{hash}";
    }

    private static string DescribeBbox(MapGenerationCitySettings city)
    {
        return FormattableString.Invariant($"{city.MinLon},{city.MinLat},{city.MaxLon},{city.MaxLat}");
    }

    private sealed record GeneratedPolygon(Polygon Polygon, long? OsmId);
    private sealed record RoadSegment(LineString Geometry, long OsmId, double WidthMeters);
}
