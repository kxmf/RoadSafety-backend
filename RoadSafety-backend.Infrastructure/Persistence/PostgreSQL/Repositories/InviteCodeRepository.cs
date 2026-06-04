using Microsoft.EntityFrameworkCore;
using RoadSafety_backend.Domain.Aggregates.InviteCodeAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;

public sealed class InviteCodeRepository(ApplicationDbContext dbContext) : IInviteCodeRepository
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<InviteCode?> GetInviteCodeByValueAsync(string code, CancellationToken cancellationToken)
    {
        return await _dbContext.InviteCodes
            .FirstOrDefaultAsync(ic => ic.Value == InviteCodeValue.Create(code), cancellationToken);
    }

    public async Task<InviteCode?> GetInviteCodeByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.InviteCodes
            .FirstOrDefaultAsync(ic => ic.Id.Value == id, cancellationToken);
    }

    public async Task CreateInviteCodeAsync(InviteCode inviteCode, CancellationToken cancellationToken)
    {
        await _dbContext.InviteCodes.AddAsync(inviteCode, cancellationToken);
    }
}
