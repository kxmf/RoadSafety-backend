using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.DeviceTokenAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Notifications;

public class DeleteDeviceTokenUseCase(
    IDeviceTokenRepository deviceTokenRepository,
    ICurrentUserAccessor userAccessor,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<bool>> ExecuteAsync(string token, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<bool>.Failure(Error.Unauthorized("User not authenticated."));

        if (string.IsNullOrWhiteSpace(token))
            return Result<bool>.Failure(Error.Validation("Device token cannot be empty."));

        var deviceToken = await deviceTokenRepository.GetByTokenAsync(token.Trim(), cancellationToken);
        if (deviceToken is not null && deviceToken.UserId == userAccessor.UserId)
        {
            deviceToken.Revoke(DateTimeOffset.UtcNow);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<bool>.Success(true);
    }
}
