using RoadSafety_backend.Application.DTOs.Requests.Auth;
using RoadSafety_backend.Application.DTOs.Responses.Auth;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Auth;

public class RefreshTokensUseCase(
    IUnitOfWork unitOfWork,
    ISessionRepository sessionRepository,
    ITokenService tokenService,
    IUserRepository userRepository)
{
    public async Task<Result<RefreshTokensResponse>> ExecuteAsync(RefreshTokensRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result<RefreshTokensResponse>.Failure(Error.Validation("Refresh token cannot be empty"));

        var refreshTokenHash = tokenService.HashToken(request.RefreshToken);

        var session = await sessionRepository.GetSessionByRefreshTokenHashAsync(refreshTokenHash, cancellationToken);

        if (session == null)
            return Result<RefreshTokensResponse>.Failure(Error.Unauthorized("Invalid refresh token"));

        if (session.IsRevoked)
            return Result<RefreshTokensResponse>.Failure(Error.Unauthorized("Session is revoked"));

        var refreshToken = session.RefreshTokens.FirstOrDefault(rt => rt.TokenHash == refreshTokenHash);
        if (refreshToken == null || !refreshToken.IsActive)
            return Result<RefreshTokensResponse>.Failure(Error.Unauthorized("Invalid refresh token"));

        var user = await userRepository.GetUserByIdAsync(session.UserId, cancellationToken);

        if (user == null)
            return Result<RefreshTokensResponse>.Failure(Error.NotFound("User not found"));

        var (plainRefreshToken, newRefreshToken) = tokenService.GenerateRefreshToken(session.UserId, session.Id);

        session.RotateRefreshToken(newRefreshToken);

        await sessionRepository.UpdateSessionAsync(session, cancellationToken);

        var (accessToken, accessTokenExpirationDateTime) = tokenService.GenerateAccessToken(user);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var refreshTokensResponse = new RefreshTokensResponse(
            accessToken,
            accessTokenExpirationDateTime,
            plainRefreshToken,
            newRefreshToken.ExpiresAt);

        return Result<RefreshTokensResponse>.Success(refreshTokensResponse);
    }
}
