## Архитектура

### Обзор
Проект построен на основе **Clean Architecture**

Код разделен на четыре проекта (Domain, Application, Infrastructure. Presentation) и структура выглядит подобным образом:

```
RoadSafety-backend/
├── RoadSafety-backend.Domain/
|   ├── Common/
|   |   ├── Result.cs
|   |   └── Error.cs
|   └── Aggregates/
|       ├── FamilyAggregate/
|       |   ├── Family.cs (Root)
|       |   ├── FamilyId.cs (VO)
|       |   ├── FamilyMember.cs (Entity)
|       |   ├── FamilyMemberRole.cs (Enum)
|       |   └── IFamilyRepository.cs (Interface)   
|       ├── UserAggregate/
|       |   ├── User.cs (Root)
|       |   ├── UserId.cs (VO)
|       |   ├── UserContacts.cs (VO)
|       |   ├── UserProfile.cs (VO)
|       |   ├── PhoneNumber (VO)
|       |   └── IUserRepository.cs (Interface)
|       └── SessionAggregate/
|           ├── Session.cs (Root)
|           ├── RefreshToken.cs (Entity)
|           ├── SessionId.cs (VO)
|           ├── RefreshTokenId.cs (VO)
|           └── ISessionRepository.cs (Interface)
├── RoadSafety-backend.Application/
|   ├── DTOs/
|   |   ├── Requests/
|   |   |   ├── Auth/
|   |   |   |   ├── RegisterRequest.cs
|   |   |   |   ├── LoginRequest.cs
|   |   |   |   └── LogOutRequest.cs
|   |   |   ├── User/
|   |   |   |   ├── UpdatePasswordRequest.cs
|   |   |   |   ├── UpdateEmailRequest.cs
|   |   |   |   ├── UpdatePhoneNumberRequest.cs
|   |   |   |   └── UpdateProfileRequest.cs
|   |   |   └── Family/
|   |   |       ├── FamilyCreateRequest.cs
|   |   |       ├── InviteCreateRequest.cs
|   |   |       └── FamilyJoinRequest.cs
|   |   └── Responses/
|   |       ├── Auth/
|   |       |   ├── RegisterResponse.cs
|   |       |   ├── LoginResponse.cs
|   |       |   └── LogOutResponse.cs
|   |       ├── User
|   |       |   ├── UpdatePasswordResponse.cs
|   |       |   ├── UpdateEmailResponse.cs
|   |       |   ├── UpdatePhoneNumberResponse.cs
|   |       |   └── UpdateProfileResponse.cs
|   |       └── Family/
|   |           ├── FamilyResponse.cs
|   |           ├── FamilyDetailsResponse.cs
|   |           ├── FamilyMemberResponse.cs
|   |           └── InviteCodeResponse.cs
|   ├── UseCases/
|   |   ├── Auth/
|   |   |   ├── RegisterUseCase.cs
|   |   |   ├── LoginUseCase.cs
|   |   |   ├── RefreshTokenUseCase.cs
|   |   |   └── LogOutUseCase.cs
|   |   ├── User/
|   |   |   ├── UpdatePasswordUseCase.cs
|   |   |   ├── UpdateEmailUseCase.cs
|   |   |   ├── UpdatePhoneNumberUseCase.cs
|   |   |   └── UpdateProfileUseCase.cs
|   |   └── Family/
|   |       ├── CreateFamilyUseCase.cs
|   |       ├── GetFamilyUseCase.cs
|   |       ├── ListFamilyMembersUseCase.cs
|   |       ├── CreateInviteCodeUseCase.cs
|   |       └── JoinFamilyByInviteCodeUseCase.cs
|   └── Interfaces/
|       ├── ITokenService.cs
|       ├── IUnitOfWork.cs
|       └── IPasswordHasher.cs
├── RoadSafety-backend.Infrastucture/
|   └── Persistence/
|       └── PostgreSQL/
|       |   ├── Configurations/
|       |   |   ├── SessionConfiguration.cs
|       |   |   ├── RefreshTokenConfiguration.cs
|       |   |   ├── FamilyConfiguration.cs
|       |   |   ├── FamilyMemberConfiguration.cs
|       |   |   ├── UserConfiguration.cs
|       |   |   └── InviteCodeConfiguration.cs
|       |   ├── Context/
|       |   |   └── ApplicationDbContext.cs
|       |   ├── Repositories/
|       |   |   ├── UnifOfWork.cs
|       |   |   ├── SessionRepository.cs
|       |   |   ├── UserRepository.cs
|       |   |   ├── FamilyRepository.cs
|       |   |   └── InviteCodeRepository.cs
|       |   └── Migrations/
|       └── Services/
|           ├── Settings/
|           |   └── JwtSettings.cs
|           ├── PasswordHasher.cs
|           └── JwtTokenService.cs
└── RoadSafety-backend.Presentation/
    ├── Controllers/
    |   ├── UsersController.cs
    |   ├── FamilyController.cs
    |   └── AuthController.cs
    ├── Middlewares/
    ├── Filters/
    └── Services/
```