using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.DeviceTokenAggregate;

public interface IDeviceTokenRepository
{
    Task<DeviceToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
    Task<List<DeviceToken>> GetActiveByUserIdsAsync(IEnumerable<UserId> userIds, CancellationToken cancellationToken);
    Task AddAsync(DeviceToken deviceToken, CancellationToken cancellationToken);
}
