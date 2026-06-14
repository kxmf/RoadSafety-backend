using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;

public sealed class UserMapAreaRepository(ApplicationDbContext dbContext) : IUserMapAreaRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<List<UserMapArea>> GetByFamilyAsync(FamilyId familyId, UserId? childId, CancellationToken cancellationToken)
    {
        return await _dbContext.UserMapAreas
            .AsNoTracking()
            .Where(area => area.FamilyId == familyId && (area.ChildId == null || area.ChildId == childId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<UserMapArea>> GetIntersectingCustomAreasAsync(FamilyId familyId, UserId? childId, Polygon bbox, CancellationToken cancellationToken)
    {
        return await _dbContext.UserMapAreas
            .AsNoTracking()
            .Where(area =>
                area.FamilyId == familyId &&
                (area.ChildId == null || area.ChildId == childId) &&
                area.Geometry != null &&
                area.Geometry.Intersects(bbox))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserMapArea?> GetBaseOverrideAsync(FamilyId familyId, UserId? childId, string baseAreaKey, CancellationToken cancellationToken)
    {
        return await _dbContext.UserMapAreas
            .Where(area =>
                area.FamilyId == familyId &&
                area.ChildId == childId &&
                area.BaseAreaKey == baseAreaKey &&
                area.Geometry == null)
            .OrderByDescending(area => area.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserMapArea> CreateAsync(UserMapArea userMapArea, CancellationToken cancellationToken)
    {
        await _dbContext.UserMapAreas.AddAsync(userMapArea, cancellationToken);

        return userMapArea;
    }

    public async Task DeleteFamilyAreasAsync(FamilyId familyId, CancellationToken cancellationToken)
    {
        var areas = await _dbContext.UserMapAreas
            .Where(area => area.FamilyId == familyId)
            .ToListAsync(cancellationToken);

        _dbContext.UserMapAreas.RemoveRange(areas);
    }
}
