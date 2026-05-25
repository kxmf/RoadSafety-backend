using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Application.Interfaces;

public interface ICurrentUserAccessor
{
    UserId? UserId { get; }

    bool IsAuthenticated { get; }
}
