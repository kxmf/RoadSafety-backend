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
            .AsNoTracking()
            .FirstOrDefaultAsync(ic => EF.Functions.Collate(ic.Value.Value, "C") == code, cancellationToken);
    }

    public async Task<InviteCode?> GetInviteCodeByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.InviteCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(ic => ic.Id.Value == id, cancellationToken);
    }

    public async Task CreateInviteCodeAsync(InviteCode inviteCode, CancellationToken cancellationToken)
    {
        await _dbContext.InviteCodes.AddAsync(inviteCode, cancellationToken);
    }

    public async Task UpdateInviteCodeAsync(InviteCode inviteCode, CancellationToken cancellationToken)
    {
        _dbContext.InviteCodes.Update(inviteCode);
        await Task.CompletedTask;
    }
}
