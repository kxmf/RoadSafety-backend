using NetTopologySuite.Geometries;

namespace RoadSafety_backend.Infrastructure.Services.MapGeneration;

internal static class WebMercatorProjection
{
    private const double OriginShift = 20037508.342789244;
    private const double MaxLatitude = 85.05112878;

    private static readonly GeometryFactory Wgs84Factory = new(new PrecisionModel(), 4326);
    private static readonly GeometryFactory WebMercatorFactory = new(new PrecisionModel(), 3857);

    public static Coordinate ToWebMercator(Coordinate coordinate)
    {
        var lon = Math.Clamp(coordinate.X, -180, 180);
        var lat = Math.Clamp(coordinate.Y, -MaxLatitude, MaxLatitude);

        var x = lon * OriginShift / 180.0;
        var y = Math.Log(Math.Tan((90.0 + lat) * Math.PI / 360.0)) / (Math.PI / 180.0);
        y = y * OriginShift / 180.0;

        return new Coordinate(x, y);
    }

    public static Coordinate ToWgs84(Coordinate coordinate)
    {
        var lon = coordinate.X / OriginShift * 180.0;
        var lat = coordinate.Y / OriginShift * 180.0;
        lat = 180.0 / Math.PI * (2.0 * Math.Atan(Math.Exp(lat * Math.PI / 180.0)) - Math.PI / 2.0);

        return new Coordinate(lon, lat);
    }

    public static LineString ToWebMercator(LineString line)
    {
        return WebMercatorFactory.CreateLineString(line.Coordinates.Select(ToWebMercator).ToArray());
    }

    public static Polygon CreateWgs84Bbox(MapGenerationCitySettings city)
    {
        return Wgs84Factory.CreatePolygon([
            new Coordinate(city.MinLon, city.MinLat),
            new Coordinate(city.MaxLon, city.MinLat),
            new Coordinate(city.MaxLon, city.MaxLat),
            new Coordinate(city.MinLon, city.MaxLat),
            new Coordinate(city.MinLon, city.MinLat)
        ]);
    }

    public static Polygon CreateWebMercatorBbox(MapGenerationCitySettings city)
    {
        return TransformPolygon(CreateWgs84Bbox(city), ToWebMercator, WebMercatorFactory);
    }

    public static Polygon ToWgs84Polygon(Polygon polygon)
    {
        return TransformPolygon(polygon, ToWgs84, Wgs84Factory);
    }

    public static Polygon CreateWebMercatorRectangle(double minX, double minY, double maxX, double maxY)
    {
        return WebMercatorFactory.CreatePolygon([
            new Coordinate(minX, minY),
            new Coordinate(maxX, minY),
            new Coordinate(maxX, maxY),
            new Coordinate(minX, maxY),
            new Coordinate(minX, minY)
        ]);
    }

    private static Polygon TransformPolygon(Polygon polygon, Func<Coordinate, Coordinate> transform, GeometryFactory factory)
    {
        var shell = factory.CreateLinearRing(polygon.ExteriorRing.Coordinates.Select(transform).ToArray());
        var holes = new LinearRing[polygon.NumInteriorRings];

        for (var i = 0; i < polygon.NumInteriorRings; i++)
            holes[i] = factory.CreateLinearRing(polygon.GetInteriorRingN(i).Coordinates.Select(transform).ToArray());

        return factory.CreatePolygon(shell, holes);
    }
}
