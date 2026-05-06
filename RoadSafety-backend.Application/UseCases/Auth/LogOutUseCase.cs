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
        var tokenHash = tokenService.HashToken(request.RefreshToken);

        var session = await sessionRepository.GetSessionByRefreshTokenHashAsync(tokenHash, cancellationToken);

        if (session == null)
        {
            return Result<LogOutResponse>.Failure(new Error(ErrorType.Unauthorized, "Invalid refresh token"));
        }

        session.RevokeAll();
        await sessionRepository.UpdateSessionAsync(session, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LogOutResponse>.Success(new LogOutResponse());
    }
}