using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;
using FamilyAggregateRoot = RoadSafety_backend.Domain.Aggregates.FamilyAggregate.Family;

namespace RoadSafety_backend.Application.UseCases.Maps;

internal static class FamilyMapAccessValidator
{
    public static async Task<Result<(FamilyAggregateRoot Family, UserId? ChildId)>> ValidateReadAsync(
        IFamilyRepository familyRepository,
        ICurrentUserAccessor userAccessor,
        Guid familyId,
        Guid? childId,
        CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<(FamilyAggregateRoot, UserId?)>.Failure(Error.Unauthorized("User not authenticated."));

        if (familyId == Guid.Empty)
            return Result<(FamilyAggregateRoot, UserId?)>.Failure(Error.Validation("familyId is required."));

        if (childId == Guid.Empty)
            return Result<(FamilyAggregateRoot, UserId?)>.Failure(Error.Validation("childId cannot be empty."));

        var family = await familyRepository.GetFamilyByIdAsync(new FamilyId(familyId), cancellationToken);
        if (family is null)
            return Result<(FamilyAggregateRoot, UserId?)>.Failure(Error.NotFound("Family not found."));

        var currentUserId = userAccessor.UserId;
        if (family.CreatedByUserId != currentUserId && !family.Members.Any(m => m.UserId == currentUserId))
            return Result<(FamilyAggregateRoot, UserId?)>.Failure(Error.Forbidden("User is not a member of the family."));

        var requestedChildId = childId is null ? null : new UserId(childId.Value);
        if (requestedChildId is not null && !family.Members.Any(m => m.UserId == requestedChildId))
            return Result<(FamilyAggregateRoot, UserId?)>.Failure(Error.Validation("childId must belong to the family."));

        return Result<(FamilyAggregateRoot, UserId?)>.Success((family, requestedChildId));
    }

    public static async Task<Result<(FamilyAggregateRoot Family, UserId? ChildId, UserId CurrentUserId)>> ValidateParentWriteAsync(
        IFamilyRepository familyRepository,
        ICurrentUserAccessor userAccessor,
        Guid familyId,
        Guid? childId,
        CancellationToken cancellationToken)
    {
        var readResult = await ValidateReadAsync(familyRepository, userAccessor, familyId, childId, cancellationToken);
        if (!readResult.IsSuccess)
            return Result<(FamilyAggregateRoot, UserId?, UserId)>.Failure(readResult.Error);

        var currentUserId = userAccessor.UserId!;
        var family = readResult.Value.Family;
        var currentMember = family.Members.FirstOrDefault(m => m.UserId == currentUserId);
        if (currentMember is not null && currentMember.Role != FamilyMemberRole.Parent)
            return Result<(FamilyAggregateRoot, UserId?, UserId)>.Failure(Error.Forbidden("Only parents can change user map areas."));

        return Result<(FamilyAggregateRoot, UserId?, UserId)>.Success((family, readResult.Value.ChildId, currentUserId));
    }
}
