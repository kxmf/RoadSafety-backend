using NetTopologySuite.Geometries;

namespace RoadSafety_backend.Infrastructure.Services.MapGeneration;

internal sealed record OsmWay(long Id, IReadOnlyDictionary<string, string> Tags, LineString Geometry);
internal sealed record OsmNode(long Id, IReadOnlyDictionary<string, string> Tags, Point Geometry);
