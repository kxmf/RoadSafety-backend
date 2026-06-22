using System.Text.Json.Serialization;

namespace RoadSafety_backend.Domain.Aggregates.MapAggregate;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RiskLevel
{
    Green,
    Yellow,
    Red
}
