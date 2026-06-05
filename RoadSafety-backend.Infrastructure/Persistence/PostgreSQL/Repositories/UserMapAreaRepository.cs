using Microsoft.EntityFrameworkCore;
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
            .Where(area => area.FamilyId == familyId && area.ChildId == childId)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserMapArea> CreateAsync(UserMapArea userMapArea, CancellationToken cancellationToken)
    {
        await _dbContext.UserMapAreas.AddAsync(userMapArea, cancellationToken);

        return userMapArea;
    }
}
