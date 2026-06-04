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
        string? email = null;
        string? phoneNumber = null;

        bool isEmail = request.Login.Contains('@');

        if (isEmail)
        {
            email = request.Login;
            var existingEmailUser = await userRepository.GetUserByEmailAsync(email, cancellationToken);

            if (existingEmailUser != null)
                return Result<AuthResponse>.Failure(Error.Conflict("Email already used"));
        }
        else
        {
            phoneNumber = request.Login;
            var existingPhoneUser = await userRepository.GetUserByPhoneAsync(phoneNumber, cancellationToken);

            if (existingPhoneUser != null)
                return Result<AuthResponse>.Failure(Error.Conflict("Phone already used"));
        }

        var contactsResult = UserContacts.Create(email, phoneNumber);
        if (contactsResult.IsFailure)
            return Result<AuthResponse>.Failure(contactsResult.Error);

        var hashedPassword = passwordService.Hash(request.Password);
        var userId = UserId.New();

        var user = User.Create(userId, hashedPassword, contactsResult.Value);
        await userRepository.CreateUserAsync(user, cancellationToken);

        var sessionId = SessionId.New();
        var (plainRefreshToken, refreshToken) = tokenService.GenerateRefreshToken(user.Id, sessionId);

        var session = Session.Create(sessionId, user.Id, refreshToken);

        await sessionRepository.CreateSessionAsync(session, cancellationToken);
        
        var (accessToken, accessTokenExpirationDateTime) = tokenService.GenerateAccessToken(user);
        
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