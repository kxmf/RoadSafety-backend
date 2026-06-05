using NetTopologySuite.Geometries;
using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

internal static class MapGeometryMapper
{
    private const int Wgs84Srid = 4326;
    private static readonly GeometryFactory GeometryFactory = new(new PrecisionModel(), Wgs84Srid);

    public static Result<Polygon> CreateBboxPolygon(string bbox)
    {
        if (string.IsNullOrWhiteSpace(bbox))
            return Result<Polygon>.Failure(Error.Validation("bbox is required."));

        var parts = bbox.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length != 4)
            return Result<Polygon>.Failure(Error.Validation("bbox must contain four comma-separated numbers."));

        var values = new double[4];
        for (var i = 0; i < parts.Length; i++)
        {
            if (!double.TryParse(parts[i], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out values[i]))
                return Result<Polygon>.Failure(Error.Validation("bbox must contain valid numbers."));
        }

        var minLon = values[0];
        var minLat = values[1];
        var maxLon = values[2];
        var maxLat = values[3];

        if (minLon < -180 || maxLon > 180 || minLat < -90 || maxLat > 90)
            return Result<Polygon>.Failure(Error.Validation("bbox coordinates must be valid WGS84 longitude and latitude values."));

        if (minLon >= maxLon || minLat >= maxLat)
            return Result<Polygon>.Failure(Error.Validation("bbox minimum coordinates must be less than maximum coordinates."));

        return Result<Polygon>.Success(CreatePolygon([
            new Coordinate(minLon, minLat),
            new Coordinate(maxLon, minLat),
            new Coordinate(maxLon, maxLat),
            new Coordinate(minLon, maxLat),
            new Coordinate(minLon, minLat)
        ]));
    }

    public static Result<Polygon> FromGeoJsonPolygon(GeoJsonGeometryDto? geometry)
    {
        if (geometry is null)
            return Result<Polygon>.Failure(Error.Validation("geometry is required."));

        if (!string.Equals(geometry.Type, "Polygon", StringComparison.Ordinal))
            return Result<Polygon>.Failure(Error.Validation("Only GeoJSON Polygon geometry is supported."));

        if (geometry.Coordinates.Length == 0)
            return Result<Polygon>.Failure(Error.Validation("Polygon must contain at least one linear ring."));

        var shellResult = CreateLinearRing(geometry.Coordinates[0]);
        if (!shellResult.IsSuccess)
            return Result<Polygon>.Failure(shellResult.Error);

        var holes = new LinearRing[Math.Max(0, geometry.Coordinates.Length - 1)];
        for (var i = 1; i < geometry.Coordinates.Length; i++)
        {
            var holeResult = CreateLinearRing(geometry.Coordinates[i]);
            if (!holeResult.IsSuccess)
                return Result<Polygon>.Failure(holeResult.Error);

            holes[i - 1] = holeResult.Value;
        }

        var polygon = GeometryFactory.CreatePolygon(shellResult.Value, holes);
        if (!polygon.IsValid)
            return Result<Polygon>.Failure(Error.Validation("Polygon geometry is invalid."));

        return Result<Polygon>.Success(polygon);
    }

    public static GeoJsonGeometryDto ToGeoJson(Polygon polygon)
    {
        var rings = new List<double[][]> { ToCoordinates(polygon.ExteriorRing) };
        for (var i = 0; i < polygon.NumInteriorRings; i++)
            rings.Add(ToCoordinates(polygon.GetInteriorRingN(i)));

        return new GeoJsonGeometryDto("Polygon", rings.ToArray());
    }

    private static Result<LinearRing> CreateLinearRing(double[][] ring)
    {
        if (ring.Length < 4)
            return Result<LinearRing>.Failure(Error.Validation("Polygon linear rings must contain at least four positions."));

        var coordinates = new Coordinate[ring.Length];
        for (var i = 0; i < ring.Length; i++)
        {
            if (ring[i].Length < 2)
                return Result<LinearRing>.Failure(Error.Validation("Polygon coordinates must contain longitude and latitude."));

            var lon = ring[i][0];
            var lat = ring[i][1];

            if (lon < -180 || lon > 180 || lat < -90 || lat > 90)
                return Result<LinearRing>.Failure(Error.Validation("Polygon coordinates must be valid WGS84 longitude and latitude values."));

            coordinates[i] = new Coordinate(lon, lat);
        }

        if (!coordinates[0].Equals2D(coordinates[^1]))
            return Result<LinearRing>.Failure(Error.Validation("Polygon linear rings must be closed."));

        return Result<LinearRing>.Success(GeometryFactory.CreateLinearRing(coordinates));
    }

    private static Polygon CreatePolygon(Coordinate[] coordinates)
    {
        var shell = GeometryFactory.CreateLinearRing(coordinates);
        return GeometryFactory.CreatePolygon(shell);
    }

    private static double[][] ToCoordinates(LineString ring)
    {
        return ring.Coordinates
            .Select(c => new[] { c.X, c.Y })
            .ToArray();
    }
}
