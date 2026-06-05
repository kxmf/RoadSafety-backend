using System.Globalization;

namespace RoadSafety_backend.Infrastructure.Services.MapGeneration;

internal static class OsmRoadClassifier
{
    private static readonly HashSet<string> RoadHighways =
    [
        "motorway",
        "trunk",
        "primary",
        "secondary",
        "tertiary",
        "unclassified",
        "residential",
        "service",
        "living_street"
    ];

    public static bool IsRoad(IReadOnlyDictionary<string, string> tags)
    {
        return tags.TryGetValue("highway", out var highway) && RoadHighways.Contains(highway);
    }

    public static bool IsCrossing(IReadOnlyDictionary<string, string> tags)
    {
        return tags.TryGetValue("highway", out var highway) && highway == "crossing"
               || tags.TryGetValue("footway", out var footway) && footway == "crossing";
    }

    public static bool IsPedestrianPathToKeep(IReadOnlyDictionary<string, string> tags)
    {
        if (!tags.TryGetValue("highway", out var highway))
            return false;

        if (highway is "pedestrian" or "living_street" or "steps")
            return true;

        if (highway is not ("footway" or "path"))
            return false;

        if (tags.TryGetValue("informal", out var informal) && informal == "yes")
            return false;

        if (tags.TryGetValue("footway", out var footway) && footway is "sidewalk" or "crossing")
            return false;

        return true;
    }

    public static double GetWidthMeters(IReadOnlyDictionary<string, string> tags)
    {
        if (tags.TryGetValue("width", out var widthText) && TryParsePositiveDouble(widthText, out var width))
            return width + 2.0;

        if (tags.TryGetValue("lanes", out var lanesText) && int.TryParse(lanesText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var lanes) && lanes > 0)
            return lanes * 3.5 + 2.0;

        var type = tags.GetValueOrDefault("highway", "residential");
        return type switch
        {
            "motorway" => 24.0,
            "trunk" => 20.0,
            "primary" => 18.0,
            "secondary" => 14.0,
            "tertiary" => 11.0,
            "residential" => 7.5,
            "service" => 5.0,
            "living_street" => 5.0,
            "pedestrian" => 4.0,
            "path" => 1.5,
            "footway" => 1.5,
            "steps" => 1.5,
            _ => 5.0
        };
    }

    private static bool TryParsePositiveDouble(string text, out double value)
    {
        var normalized = text.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value) && value > 0;
    }
}
