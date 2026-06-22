using RoadSafety_backend.Application.DTOs.Requests.Auth;
using RoadSafety_backend.Application.DTOs.Responses.Auth;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Auth;

public class LogOutUseCase(
    IUnitOfWork unitOfWork,
    ISessionRepository sessionRepository,
    ITokenService tokenService)
{
    public async Task<Result<LogOutResponse>> ExecuteAsync(LogOutRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result<LogOutResponse>.Failure(Error.Validation("Refresh token cannot be empty"));

        var tokenHash = tokenService.HashToken(request.RefreshToken);

        var session = await sessionRepository.GetSessionByRefreshTokenHashAsync(tokenHash, cancellationToken);
        
        if (session == null)
        {
            return Result<LogOutResponse>.Failure(Error.Unauthorized("Invalid refresh token"));
        }

        session.Revoke();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LogOutResponse>.Success(new LogOutResponse());
    }
}
