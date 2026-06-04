namespace RoadSafety_backend.Domain.Aggregates.InviteCodeAggregate;

public interface IInviteCodeRepository
{
    public Task<InviteCode?> GetInviteCodeByValueAsync(string code, CancellationToken cancellationToken);
    public Task<InviteCode?> GetInviteCodeByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task CreateInviteCodeAsync(InviteCode inviteCode, CancellationToken cancellationToken);

}
