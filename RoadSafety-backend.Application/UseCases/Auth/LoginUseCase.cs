using RoadSafety_backend.Application.DTOs.Requests.Auth;
using RoadSafety_backend.Application.DTOs.Responses.Auth;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Auth;

public class LoginUseCase(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    IPasswordService passwordService,
    ITokenService tokenService)
{
    public async Task<Result<AuthResponse>> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        User? user = null;

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            user = await userRepository.GetUserByPhoneAsync(request.PhoneNumber, cancellationToken);

        if (user == null && !string.IsNullOrWhiteSpace(request.Email))
            user = await userRepository.GetUserByEmailAsync(request.Email, cancellationToken);

        if (user == null)
            return Result<AuthResponse>.Failure(new Error(ErrorType.Unauthorized, "Incorrect login or password"));

        if (!passwordService.Verify(request.Password, user.HashedPassword))
            return Result<AuthResponse>.Failure(new Error(ErrorType.Unauthorized, "Incorrect login or password"));

        var (accessToken, accessTokenExpirationDateTime) = tokenService.GenerateAccessToken(user);
        var sessionId = SessionId.New();
        var (plainRefreshToken, refreshToken) = tokenService.GenerateRefreshToken(user.Id, sessionId);
        var session = Session.Create(sessionId, user.Id, refreshToken);
        await sessionRepository.CreateSessionAsync(session, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var authResponse = new AuthResponse(
            user.Id,
            accessToken,
            accessTokenExpirationDateTime,
            plainRefreshToken,
            refreshToken.ExpiresAt);

        return Result<AuthResponse>.Success(authResponse);
    }
}
