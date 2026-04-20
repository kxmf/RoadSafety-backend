## Архитектура

### Обзор
Проект построен на основе **Clean Architecture**

Код разделен на четыре проекта (Domain, Application, Infrastructure. Presentation) и структура выглядит подобным образом:

```
RoadSafety-backend
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
|   ├── UseCases/
|   |   ├── Auth/
|   |   |   ├── RegisterUserUseCase.cs
|   |   |   ├── LoginUseCase.cs
|   |   |   ├── RefreshTokenUseCase.cs
|   |   |   └── LogOutUseCase.cs
|   |   ├── User/
|   |   └── Family/
|   └── Interfaces/
├── RoadSafety-backend.Infrastucture/
|   └── Persistence/
|       └── PostgreSQL/
|           ├── Configurations/
|           |   ├── FamilyConfiguration.cs
|           |   ├── FamilyMemberConfiguration.cs
|           |   └── UserConfiguration.cs
|           ├── Context/
|           |   └── ApplicationDbContext.cs
|           ├── Repositories/
|           |   ├── UserRepository.cs
|           |   └── FamilyRepository.cs
|           └── Migrations
└── RoadSafety-backend.Presentation/
    ├── Controllers/
    |   ├── UsersController.cs
    |   ├── FamilyController.cs
    |   └── AuthController.cs
    ├── Middlewares/
    ├── Filters/
    └── Services/
```