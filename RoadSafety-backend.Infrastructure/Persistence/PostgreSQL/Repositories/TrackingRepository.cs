using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.TrackingAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;

public sealed class TrackingRepository(ApplicationDbContext dbContext) : ITrackingRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<ChildLocation?> GetLocationAsync(UserId childId, CancellationToken cancellationToken)
    {
        return await _dbContext.ChildLocations
            .AsNoTracking()
            .FirstOrDefaultAsync(location => location.ChildId == childId, cancellationToken);
    }

    public async Task<List<ChildLocation>> GetLocationsAsync(IReadOnlyCollection<UserId> childIds, CancellationToken cancellationToken)
    {
        return await _dbContext.ChildLocations
            .AsNoTracking()
            .Where(location => childIds.Contains(location.ChildId))
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertLocationAsync(ChildLocation location, CancellationToken cancellationToken)
    {
        var current = await _dbContext.ChildLocations
            .FirstOrDefaultAsync(item => item.ChildId == location.ChildId, cancellationToken);

        if (current is null)
        {
            await _dbContext.ChildLocations.AddAsync(location, cancellationToken);
            return;
        }

        current.Update(
            location.Location,
            location.AccuracyMeters,
            location.CurrentRisk,
            location.MatchedUserAreaId,
            location.MatchedBaseAreaKey,
            location.RecordedAt,
            location.LastUpdatedAt);
    }

    public async Task<ChildStats?> GetStatsAsync(UserId childId, CancellationToken cancellationToken)
    {
        return await _dbContext.ChildStats
            .AsNoTracking()
            .FirstOrDefaultAsync(stats => stats.ChildId == childId, cancellationToken);
    }

    public async Task<ChildStats> EnsureStatsAsync(UserId childId, CancellationToken cancellationToken)
    {
        var stats = await _dbContext.ChildStats
            .FirstOrDefaultAsync(item => item.ChildId == childId, cancellationToken);

        if (stats is not null)
            return stats;

        stats = ChildStats.Create(childId);
        await _dbContext.ChildStats.AddAsync(stats, cancellationToken);
        return stats;
    }

    public async Task<ChildRiskState?> GetRiskStateAsync(UserId childId, CancellationToken cancellationToken)
    {
        return await _dbContext.ChildRiskStates
            .FirstOrDefaultAsync(state => state.ChildId == childId, cancellationToken);
    }

    public async Task UpsertRiskStateAsync(ChildRiskState state, CancellationToken cancellationToken)
    {
        if (_dbContext.Entry(state).State != EntityState.Detached)
            return;

        var current = await _dbContext.ChildRiskStates
            .FirstOrDefaultAsync(item => item.ChildId == state.ChildId, cancellationToken);

        if (current is null)
            await _dbContext.ChildRiskStates.AddAsync(state, cancellationToken);
    }

    public async Task<RiskMatch> GetRiskMatchAsync(
        FamilyId familyId,
        string cityId,
        UserId childId,
        Point location,
        CancellationToken cancellationToken)
    {
        var userArea = await _dbContext.UserMapAreas
            .AsNoTracking()
            .Where(area =>
                area.FamilyId == familyId &&
                area.ChildId == childId &&
                area.Geometry != null &&
                area.Geometry.Contains(location))
            .OrderByDescending(area => area.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? await _dbContext.UserMapAreas
                .AsNoTracking()
                .Where(area =>
                    area.FamilyId == familyId &&
                    area.ChildId == null &&
                    area.Geometry != null &&
                    area.Geometry.Contains(location))
                .OrderByDescending(area => area.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

        if (userArea is not null)
            return new RiskMatch(userArea.Risk, userArea.Id.Value, null);

        var baseArea = await _dbContext.MapAreas
            .AsNoTracking()
            .Where(area =>
                area.CityId == cityId &&
                area.Geometry.Contains(location))
            .OrderByDescending(area => area.Risk == RiskLevel.Red)
            .ThenByDescending(area => area.Risk == RiskLevel.Yellow)
            .FirstOrDefaultAsync(cancellationToken);

        if (baseArea is null)
            return new RiskMatch(RiskLevel.Green, null, null);

        var overrideArea = await _dbContext.UserMapAreas
            .AsNoTracking()
            .Where(area =>
                area.FamilyId == familyId &&
                area.ChildId == childId &&
                area.BaseAreaKey == baseArea.BaseAreaKey &&
                area.Geometry == null)
            .OrderByDescending(area => area.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? await _dbContext.UserMapAreas
                .AsNoTracking()
                .Where(area =>
                    area.FamilyId == familyId &&
                    area.ChildId == null &&
                    area.BaseAreaKey == baseArea.BaseAreaKey &&
                    area.Geometry == null)
                .OrderByDescending(area => area.UpdatedAt)
                .FirstOrDefaultAsync(cancellationToken);

        return overrideArea is null
            ? new RiskMatch(baseArea.Risk, null, baseArea.BaseAreaKey)
            : new RiskMatch(overrideArea.Risk, overrideArea.Id.Value, baseArea.BaseAreaKey);
    }
}
