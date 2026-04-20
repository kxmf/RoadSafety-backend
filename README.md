## Архитектура

### Обзор
Проект построен на основе **Clean Architecture**

Код разделен на четыре проекта (Domain, Application, Infrastructure. Presentation) и структура выглядит подобным образом:

```
RoadSafety-backend/
├── RoadSafety-backend.Domain/
|   └── Aggregates/
|       ├── FamilyAggregate/
|       |   ├── Family.cs (Root)
|       |   ├── FamilyId.cs (VO)
|       |   ├── FamilyMember.cs (Entity)
|       |   ├── FamilyMemberRole.cs (Enum)
|       |   ├── FamilyCode.cs (VO)
|       |   └── IFamilyRepository.cs (Interface)
|       └── UserAggregate/
|           ├── User.cs (Root)
|           ├── UserId.cs (VO)
|           ├── UserContacts.cs (VO)
|           ├── UserProfile.cs (VO)
|           ├── PhoneNumber (VO)
|           ├── Password (VO)
|           └── IUserRepository.cs (Interface)
├── RoadSafety-backend.Application/
|   ├── DTOs/
|   |   ├── Requests/
|   |   |   ├── Auth/
|   |   |   ├── User/
|   |   |   |   ├── UpdatePasswordRequest.cs
|   |   |   |   ├── UpdateEmailRequest.cs
|   |   |   |   ├── UpdatePhoneNumberRequest.cs
|   |   |   |   └── UpdateProfileRequest.cs
|   |   |   └── Family/
|   |   └── Responses/
|   |       ├── Auth/
|   |       ├── User
|   |       |   ├── UpdatePasswordResponse.cs
|   |       |   ├── UpdateEmailResponse.cs
|   |       |   ├── UpdatePhoneNumberResponse.cs
|   |       |   └── UpdateProfileResponse.cs
|   |       └── Family/
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
|   └── Interfaces/
├── RoadSafety-backend.Infrastucture/
|   └── Persistence/
|   |   └── PostgreSQL/
|   |      ├── Configurations/
|   |      |   ├── 
|   |      |   ├── FamilyConfiguration.cs
|   |      |   ├── FamilyMemberConfiguration.cs
|   |      |   └── UserConfiguration.cs
|   |      ├── Context/
|   |      |   └── ApplicationDbContext.cs
|   |      ├── Repositories/
|   |      |   ├── UserRepository.cs
|   |      |   └── FamilyRepository.cs
|   |      └── Migrations
|   └── Identity/
|       └── Models/
|           └── Aggregates/
|               └── SessionAggegate/
|                   ├── Session.cs (Root)
|                   ├── RefreshToken.cs (Entity)
|                   ├── SessionId.cs (VO)
|                   ├── RefreshTokenId.cs (VO)
|                   └── ISessionRepository.cs (Interface)
└── RoadSafety-backend.Presentation/
    ├── Controllers/
    |   ├── UsersController.cs
    |   ├── FamilyController.cs
    |   └── AuthController.cs
    ├── Middlewares/
    ├── Filters/
    └── Services/
```