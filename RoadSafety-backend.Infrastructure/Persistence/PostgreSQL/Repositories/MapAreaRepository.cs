using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;

public sealed class MapAreaRepository(ApplicationDbContext dbContext) : IMapAreaRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<List<MapArea>> GetIntersectingAsync(Polygon bbox, string? cityId, CancellationToken cancellationToken)
    {
        var query = _dbContext.MapAreas
            .AsNoTracking()
            .Where(area => area.Geometry.Intersects(bbox));

        if (!string.IsNullOrWhiteSpace(cityId))
            query = query.Where(area => area.CityId == cityId);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(MapAreaId id, CancellationToken cancellationToken)
    {
        return await _dbContext.MapAreas.AnyAsync(area => area.Id == id, cancellationToken);
    }

    public async Task ReplaceCityAreasAsync(string cityId, IReadOnlyCollection<MapArea> areas, CancellationToken cancellationToken)
    {
        var existingAreas = await _dbContext.MapAreas
            .Where(area => area.CityId == cityId)
            .ToListAsync(cancellationToken);

        _dbContext.MapAreas.RemoveRange(existingAreas);
        await _dbContext.MapAreas.AddRangeAsync(areas, cancellationToken);
    }
}
