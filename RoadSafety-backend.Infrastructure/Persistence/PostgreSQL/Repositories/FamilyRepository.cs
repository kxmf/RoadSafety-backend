using Microsoft.EntityFrameworkCore;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;

public sealed class FamilyRepository(ApplicationDbContext dbContext) : IFamilyRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Family?> GetFamilyByIdAsync(FamilyId id, CancellationToken cancellationToken)
    {
        return await _dbContext.Families
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Family?> GetFamilyByMemberUserIdAsync(UserId userId, CancellationToken cancellationToken)
    {
        return await _dbContext.Families
            .FirstOrDefaultAsync(f => f.Members.Any(m => m.UserId == userId), cancellationToken);
    }

    public async Task<Family> CreateFamilyAsync(Family family, CancellationToken cancellationToken)
    {
        await _dbContext.Families.AddAsync(family, cancellationToken);

        return family;
    }

    public async Task<Family> DeleteFamilyAsync(Family family, CancellationToken cancellationToken)
    {
        _dbContext.Families.Remove(family);

        return await Task.FromResult(family);
    }
}
