using RoadSafety_backend.Application.DTOs.Responses.Tracking;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.TrackingAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Tracking;

public class GetChildStatsUseCase(
    IFamilyRepository familyRepository,
    ITrackingRepository trackingRepository,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<ChildStatsResponse>> ExecuteAsync(Guid childId, CancellationToken cancellationToken)
    {
        if (childId == Guid.Empty)
            return Result<ChildStatsResponse>.Failure(Error.Validation("childId is required."));

        var access = await FamilyTrackingAccessValidator.ValidateFamilyMemberAsync(familyRepository, userAccessor, cancellationToken);
        if (!access.IsSuccess)
            return Result<ChildStatsResponse>.Failure(access.Error);

        var requestedChildId = new UserId(childId);
        var childResult = FamilyTrackingAccessValidator.ValidateChildAccess(
            access.Value.Family,
            access.Value.CurrentMember,
            access.Value.CurrentUserId,
            childId);

        if (!childResult.IsSuccess)
            return Result<ChildStatsResponse>.Failure(childResult.Error);

        if (access.Value.CurrentMember.Role != FamilyMemberRole.Parent && access.Value.CurrentUserId != requestedChildId)
            return Result<ChildStatsResponse>.Failure(Error.Forbidden("User cannot read this child stats."));

        var stats = await trackingRepository.GetStatsAsync(requestedChildId, cancellationToken);
        return Result<ChildStatsResponse>.Success(new ChildStatsResponse(
            requestedChildId.Value,
            stats?.TotalScore ?? 0,
            stats?.Rating ?? 0));
    }
}
