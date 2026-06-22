using RoadSafety_backend.Application.DTOs.Responses.Tracking;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.TrackingAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Tracking;

public class GetChildLocationUseCase(
    IFamilyRepository familyRepository,
    IUserRepository userRepository,
    ITrackingRepository trackingRepository,
    ICurrentUserAccessor userAccessor)
{
    public async Task<Result<ChildLocationResponse>> ExecuteAsync(Guid childId, CancellationToken cancellationToken)
    {
        if (childId == Guid.Empty)
            return Result<ChildLocationResponse>.Failure(Error.Validation("childId is required."));

        var access = await FamilyTrackingAccessValidator.ValidateFamilyMemberAsync(familyRepository, userAccessor, cancellationToken);
        if (!access.IsSuccess)
            return Result<ChildLocationResponse>.Failure(access.Error);

        var requestedChildId = new UserId(childId);
        var childResult = FamilyTrackingAccessValidator.ValidateChildAccess(
            access.Value.Family,
            access.Value.CurrentMember,
            access.Value.CurrentUserId,
            childId);

        if (!childResult.IsSuccess)
            return Result<ChildLocationResponse>.Failure(childResult.Error);

        if (access.Value.CurrentMember.Role != FamilyMemberRole.Parent && access.Value.CurrentUserId != requestedChildId)
            return Result<ChildLocationResponse>.Failure(Error.Forbidden("User cannot read this child location."));

        var location = await trackingRepository.GetLocationAsync(requestedChildId, cancellationToken);
        if (location is null)
            return Result<ChildLocationResponse>.Failure(Error.NotFound("Child location not found."));

        var child = await userRepository.GetUserByIdAsync(requestedChildId, cancellationToken);
        return Result<ChildLocationResponse>.Success(TrackingResponseMapper.ToResponse(location, child));
    }
}
