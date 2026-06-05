using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;

namespace RoadSafety_backend.Application.UseCases.Maps;

internal static class MapAreaResponseMapper
{
    public static MapAreaFeature ToFeature(MapArea area)
    {
        return MapAreaFeature.Create(
            MapGeometryMapper.ToGeoJson(area.Geometry),
            new MapAreaProperties(area.Id.Value, area.OsmId, area.Risk, area.CityId));
    }

    public static UserMapAreaFeature ToFeature(UserMapArea area)
    {
        return UserMapAreaFeature.Create(
            MapGeometryMapper.ToGeoJson(area.Geometry),
            new UserMapAreaProperties(
                area.Id.Value,
                area.FamilyId.Value,
                area.ChildId?.Value,
                area.BaseAreaId?.Value,
                area.Risk,
                area.CreatedByUserId.Value,
                area.CreatedAt));
    }
}
