using NetTopologySuite.Geometries;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Domain.Aggregates.TrackingAggregate;

public interface ITrackingRepository
{
    Task<ChildLocation?> GetLocationAsync(UserId childId, CancellationToken cancellationToken);
    Task<List<ChildLocation>> GetLocationsAsync(IReadOnlyCollection<UserId> childIds, CancellationToken cancellationToken);
    Task UpsertLocationAsync(ChildLocation location, CancellationToken cancellationToken);
    Task<ChildStats?> GetStatsAsync(UserId childId, CancellationToken cancellationToken);
    Task<ChildStats> EnsureStatsAsync(UserId childId, CancellationToken cancellationToken);
    Task<ChildRiskState?> GetRiskStateAsync(UserId childId, CancellationToken cancellationToken);
    Task UpsertRiskStateAsync(ChildRiskState state, CancellationToken cancellationToken);
    Task<RiskMatch> GetRiskMatchAsync(FamilyId familyId, string cityId, UserId childId, Point location, CancellationToken cancellationToken);
}

public sealed record RiskMatch(RiskLevel Risk, Guid? MatchedUserAreaId, string? MatchedBaseAreaKey);
