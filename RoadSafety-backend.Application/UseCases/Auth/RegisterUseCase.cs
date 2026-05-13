using RoadSafety_backend.Application.DTOs.Requests.Auth;
using RoadSafety_backend.Application.DTOs.Responses.Auth;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Auth;

public class RegisterUseCase(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    ISessionRepository sessionRepository,
    IPasswordService passwordService,
    ITokenService tokenService)
{
    public async Task<Result<AuthResponse>> ExecuteAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.PhoneNumber))
            return Result<AuthResponse>.Failure(Error.Validation("Email or phone number must be provided"));

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            var existingPhoneUser = await userRepository.GetUserByPhoneAsync(request.PhoneNumber, cancellationToken);
            if (existingPhoneUser != null)
                return Result<AuthResponse>.Failure(Error.Conflict("Phone already used"));
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existingEmailUser = await userRepository.GetUserByEmailAsync(request.Email, cancellationToken);
            if (existingEmailUser != null)
                return Result<AuthResponse>.Failure(Error.Conflict("Email already used"));
        }

        var hashedPassword = passwordService.Hash(request.Password);

        UserContacts userContacts;
        try
        {
            userContacts = new UserContacts(request.Email, request.PhoneNumber);
        }
        catch (ArgumentException)
        {
            return Result<AuthResponse>.Failure(Error.Validation("At least one of email or phone number must be provided"));
        }

        var user = User.Create(UserId.New(), hashedPassword, userContacts, request.Role);
        await userRepository.CreateUserAsync(user, cancellationToken);

        var sessionId = SessionId.New();
        var (plainRefreshToken, refreshToken) = tokenService.GenerateRefreshToken(user.Id, sessionId);
        var (accessToken, accessTokenExpirationDateTime) = tokenService.GenerateAccessToken(user);
        var session = Session.Create(sessionId, user.Id, refreshToken);
        await sessionRepository.CreateSessionAsync(session, cancellationToken);

        var registerResponse = new AuthResponse(
            user.Id,
            accessToken,
            accessTokenExpirationDateTime,
            plainRefreshToken,
            refreshToken.ExpiresAt);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(registerResponse);
    }
}
