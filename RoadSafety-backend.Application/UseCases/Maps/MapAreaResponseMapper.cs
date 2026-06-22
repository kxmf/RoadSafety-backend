using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;

namespace RoadSafety_backend.Application.UseCases.Maps;

internal static class MapAreaResponseMapper
{
    public static UserMapAreaFeature ToFeature(UserMapArea area)
    {
        return UserMapAreaFeature.Create(
            area.Geometry is null ? null : MapGeometryMapper.ToGeoJson(area.Geometry),
            new UserMapAreaProperties(
                area.Id.Value,
                area.FamilyId.Value,
                area.ChildId?.Value,
                area.BaseAreaKey,
                ToRiskValue(area.Risk),
                area.IsBaseOverride ? "override" : "custom",
                area.IsCustomArea ? 3 : RiskRenderPriority(area.Risk),
                area.CreatedByUserId.Value,
                area.CreatedAt,
                area.UpdatedAt));
    }

    public static string ToRiskValue(RiskLevel risk)
    {
        return risk switch
        {
            RiskLevel.Green => "green",
            RiskLevel.Yellow => "yellow",
            RiskLevel.Red => "red",
            _ => risk.ToString().ToLowerInvariant()
        };
    }

    private static int RiskRenderPriority(RiskLevel risk)
    {
        return risk switch
        {
            RiskLevel.Green => 0,
            RiskLevel.Red => 1,
            RiskLevel.Yellow => 2,
            _ => 0
        };
    }
}
