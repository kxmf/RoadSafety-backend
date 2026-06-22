using NetTopologySuite.Geometries;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.MapAggregate;

public interface IUserMapAreaRepository
{
    Task<UserMapArea?> GetByIdAsync(UserMapAreaId id, CancellationToken cancellationToken);
    Task<List<UserMapArea>> GetByFamilyAsync(FamilyId familyId, UserId? childId, CancellationToken cancellationToken);
    Task<List<UserMapArea>> GetIntersectingCustomAreasAsync(FamilyId familyId, UserId? childId, Polygon bbox, CancellationToken cancellationToken);
    Task<UserMapArea?> GetBaseOverrideAsync(FamilyId familyId, UserId? childId, string baseAreaKey, CancellationToken cancellationToken);
    Task<UserMapArea> CreateAsync(UserMapArea userMapArea, CancellationToken cancellationToken);
    Task DeleteAsync(UserMapArea userMapArea, CancellationToken cancellationToken);
    Task DeleteBaseOverrideAsync(FamilyId familyId, UserId? childId, string baseAreaKey, CancellationToken cancellationToken);
    Task DeleteFamilyAreasAsync(FamilyId familyId, CancellationToken cancellationToken);
}
