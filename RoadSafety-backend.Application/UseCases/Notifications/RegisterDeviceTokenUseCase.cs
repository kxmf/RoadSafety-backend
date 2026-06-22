using RoadSafety_backend.Application.DTOs.Requests.Notifications;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.DeviceTokenAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Notifications;

public class RegisterDeviceTokenUseCase(
    IDeviceTokenRepository deviceTokenRepository,
    ICurrentUserAccessor userAccessor,
    IUnitOfWork unitOfWork)
{
    public async Task<Result<bool>> ExecuteAsync(RegisterDeviceTokenRequest request, CancellationToken cancellationToken)
    {
        if (!userAccessor.IsAuthenticated || userAccessor.UserId is null || userAccessor.UserId == UserId.Empty)
            return Result<bool>.Failure(Error.Unauthorized("User not authenticated."));

        if (string.IsNullOrWhiteSpace(request.Token))
            return Result<bool>.Failure(Error.Validation("Device token cannot be empty."));

        var now = DateTimeOffset.UtcNow;
        var normalizedToken = request.Token.Trim();
        var deviceToken = await deviceTokenRepository.GetByTokenAsync(normalizedToken, cancellationToken);
        if (deviceToken is null)
        {
            deviceToken = DeviceToken.Create(
                Guid.NewGuid(),
                userAccessor.UserId,
                normalizedToken,
                request.Platform,
                now);

            await deviceTokenRepository.AddAsync(deviceToken, cancellationToken);
        }
        else
        {
            deviceToken.Refresh(userAccessor.UserId, request.Platform, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
