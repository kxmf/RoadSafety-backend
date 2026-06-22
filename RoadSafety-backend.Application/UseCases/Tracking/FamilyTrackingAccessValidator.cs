using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;
using FamilyAggregateRoot = RoadSafety_backend.Domain.Aggregates.FamilyAggregate.Family;

namespace RoadSafety_backend.Application.UseCases.Tracking;

internal static class FamilyTrackingAccessValidator
{
    public static async Task<Result<(FamilyAggregateRoot Family, UserId CurrentUserId, FamilyMember CurrentMember)>> ValidateFamilyMemberAsync(
        IFamilyRepository familyRepository,
        ICurrentUserAccessor userAccessor,
        CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<(FamilyAggregateRoot, UserId, FamilyMember)>.Failure(Error.Unauthorized("User not authenticated."));

        var currentUserId = userAccessor.UserId;
        var family = await familyRepository.GetFamilyByMemberUserIdAsync(currentUserId, cancellationToken);
        if (family is null)
            return Result<(FamilyAggregateRoot, UserId, FamilyMember)>.Failure(Error.NotFound("Family not found."));

        var currentMember = family.Members.FirstOrDefault(member => member.UserId == currentUserId);
        if (currentMember is null)
            return Result<(FamilyAggregateRoot, UserId, FamilyMember)>.Failure(Error.Forbidden("User is not a member of the family."));

        return Result<(FamilyAggregateRoot, UserId, FamilyMember)>.Success((family, currentUserId, currentMember));
    }

    public static Result<UserId> ValidateChildAccess(FamilyAggregateRoot family, FamilyMember currentMember, UserId currentUserId, Guid? requestedChildId)
    {
        var childId = requestedChildId is null || requestedChildId == Guid.Empty
            ? currentUserId
            : new UserId(requestedChildId.Value);

        var childMember = family.Members.FirstOrDefault(member => member.UserId == childId);
        if (childMember is null)
            return Result<UserId>.Failure(Error.Forbidden("Child is not a member of the family."));

        if (childMember.Role != FamilyMemberRole.Child)
            return Result<UserId>.Failure(Error.Validation("Requested user is not a child."));

        if (currentMember.Role == FamilyMemberRole.Child && currentUserId != childId)
            return Result<UserId>.Failure(Error.Forbidden("Children can only access their own tracking data."));

        return Result<UserId>.Success(childId);
    }

    public static Result<IReadOnlyCollection<FamilyMember>> ValidateParentChildrenRead(FamilyAggregateRoot family, FamilyMember currentMember)
    {
        if (currentMember.Role != FamilyMemberRole.Parent)
            return Result<IReadOnlyCollection<FamilyMember>>.Failure(Error.Forbidden("Only parents can read children tracking data."));

        var children = family.Members
            .Where(member => member.Role == FamilyMemberRole.Child)
            .ToList();

        return Result<IReadOnlyCollection<FamilyMember>>.Success(children);
    }
}
