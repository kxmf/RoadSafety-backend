using RoadSafety_backend.Application.DTOs.Requests.Maps;
using RoadSafety_backend.Application.DTOs.Responses.Maps;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Maps;

public sealed class GetAlertZonesUseCase(
    IFamilyRepository familyRepository,
    IMapAreaRepository mapAreaRepository,
    IUserMapAreaRepository userMapAreaRepository,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<AlertZonesResponse>> ExecuteAsync(GetAlertZonesRequest request, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<AlertZonesResponse>.Failure(Error.Unauthorized("User not authenticated."));

        if (string.IsNullOrWhiteSpace(request.CityId))
            return Result<AlertZonesResponse>.Failure(Error.Validation("cityId is required."));

        if (request.FamilyId == Guid.Empty)
            return Result<AlertZonesResponse>.Failure(Error.Validation("familyId is required."));

        if (request.ChildId == Guid.Empty)
            return Result<AlertZonesResponse>.Failure(Error.Validation("childId cannot be empty."));

        var family = await familyRepository.GetFamilyByIdAsync(new FamilyId(request.FamilyId), cancellationToken);
        if (family is null)
            return Result<AlertZonesResponse>.Failure(Error.NotFound("Family not found."));

        if (family.CreatedByUserId != userAccessor.UserId && !family.Members.Any(member => member.UserId == userAccessor.UserId))
            return Result<AlertZonesResponse>.Failure(Error.Forbidden("User is not a member of the family."));

        var childId = request.ChildId is null ? null : new UserId(request.ChildId.Value);
        if (childId is not null && !family.Members.Any(member => member.UserId == childId))
            return Result<AlertZonesResponse>.Failure(Error.Validation("childId must belong to the family."));

        var metadata = await mapAreaRepository.GetCityMetadataAsync(request.CityId, cancellationToken);
        var baseAreas = await mapAreaRepository.GetByCityAsync(request.CityId, cancellationToken);
        var userAreas = await userMapAreaRepository.GetByFamilyAsync(family.Id, childId, cancellationToken);

        var overrides = userAreas
            .Where(area => area.IsBaseOverride && area.BaseAreaKey is not null)
            .GroupBy(area => area.BaseAreaKey!)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(area => area.ChildId == childId)
                    .ThenByDescending(area => area.UpdatedAt)
                    .First());

        var zones = new List<AlertZoneResponse>();
        foreach (var baseArea in baseAreas)
        {
            var effectiveRisk = overrides.TryGetValue(baseArea.BaseAreaKey, out var userOverride)
                ? userOverride.Risk
                : baseArea.Risk;

            if (!IsAlertRisk(effectiveRisk))
                continue;

            zones.Add(new AlertZoneResponse(
                userOverride?.Id.Value ?? baseArea.Id.Value,
                baseArea.BaseAreaKey,
                MapAreaResponseMapper.ToRiskValue(effectiveRisk),
                userOverride is null ? "base" : "override",
                MapGeometryMapper.ToGeoJson(baseArea.Geometry)));
        }

        zones.AddRange(userAreas
            .Where(area => area.IsCustomArea && area.Geometry is not null && IsAlertRisk(area.Risk))
            .Select(area => new AlertZoneResponse(
                area.Id.Value,
                null,
                MapAreaResponseMapper.ToRiskValue(area.Risk),
                "custom",
                MapGeometryMapper.ToGeoJson(area.Geometry!))));

        return Result<AlertZonesResponse>.Success(new AlertZonesResponse(
            request.CityId,
            metadata?.GenerationVersion,
            zones));
    }

    private static bool IsAlertRisk(RiskLevel risk) => risk is RiskLevel.Yellow or RiskLevel.Red;
}
