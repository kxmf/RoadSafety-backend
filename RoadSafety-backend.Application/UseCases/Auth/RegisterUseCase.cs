using RoadSafety_backend.Application.DTOs.Requests;
using RoadSafety_backend.Application.DTOs.Responses;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Domain.Common;

namespace RoadSafety_backend.Application.UseCases.Auth;

public class RegisterUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IPasswordService _passwordHasher;
    private readonly ITokenService _tokenService;

    public RegisterUseCase(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        IPasswordService passwordHasher,
        ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<RegisterResponse>> ExecuteAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var existingPhoneUser = await _userRepository.GetUserByPhoneAsync(request.PhoneNumber, cancellationToken);

        if (existingPhoneUser != null)
            return Result<RegisterResponse>.Failure(new Error(409, "Email already used"));

        var existingEmailUser = await _userRepository.GetUserByEmailAsync(request.Email, cancellationToken);

        if (existingEmailUser != null)
            return Result<RegisterResponse>.Failure(new Error(409, "Phone already used"));


        var hashedPassword = _passwordHasher.Hash(request.Password);

        var userContacts = new UserContacts(request.Email, request.PhoneNumber);

        var user = new User(UserId.New(), hashedPassword, null, userContacts, null);

        await _userRepository.CreateUserAsync(user, cancellationToken);

        var refreshToken = _tokenService.GenerateRefreshToken(user.Id);

        await _sessionRepository.CreateRefreshTokenAsync(refreshToken, cancellationToken);

        var (AccessTokenHash, AccessTokenExpirationDateTime) = _tokenService.GenerateAccessToken(user, request.Role);

        var session = new Session(SessionId.New(), user.Id, refreshToken.Id, refreshToken, false);

        await _sessionRepository.CreateSessionAsync(session, cancellationToken);

        var registerResponse = new RegisterResponse(
            user.Id,
            AccessTokenHash,
            AccessTokenExpirationDateTime,
            refreshToken.TokenHash,
            refreshToken.ExpiresAt);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RegisterResponse>.Success(registerResponse);
    }
}
