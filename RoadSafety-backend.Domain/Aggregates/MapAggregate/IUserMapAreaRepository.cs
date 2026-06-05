using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.MapAggregate;

public interface IUserMapAreaRepository
{
    Task<List<UserMapArea>> GetByFamilyAsync(FamilyId familyId, UserId? childId, CancellationToken cancellationToken);
    Task<UserMapArea> CreateAsync(UserMapArea userMapArea, CancellationToken cancellationToken);
}
