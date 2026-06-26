# RoadSafety Backend

Backend проекта `RoadSafety` на .NET 10.0.

## Архитектура

Проект построен по Clean Architecture и разделен на 4 слоя:

- `RoadSafety-backend.Domain` - доменные агрегаты, value objects, `Result<T>` и бизнес-правила
- `RoadSafety-backend.Application` - use case'ы, DTO, интерфейсы сервисов и репозиториев
- `RoadSafety-backend.Infrastructure` - EF Core, PostgreSQL/PostGIS, репозитории, JWT, FCM, генерация карт
- `RoadSafety-backend.Presentation` - ASP.NET Core Web API, контроллеры, auth, OpenAPI, Scalar

### Основные доменные области

- auth: регистрация, вход, refresh/logout
- users: текущий пользователь и поиск по контакту
- families: создание семьи, приглашения, состав семьи, смена города
- maps: города, тайлы, зоны риска, пользовательские области
- tracking: отправка геолокации ребенка, текущие координаты и статистика
- notifications: уведомления и device tokens

### Сборка и зависимости

- .NET SDK 10.0
- PostgreSQL 16 с PostGIS
- `docker compose` для локальной БД и PgAdmin
- OpenAPI/Scalar для просмотра контракта
- Firebase Admin SDK для push-уведомлений

## Запуск

### 1. Установить зависимости

- .NET SDK 10.0
- Docker Desktop или совместимый Docker Engine

### 2. Подготовить конфигурацию

Скопировать `.env.example` в `.env` и проверить значения:

- `ConnectionStrings__DefaultConnection`
- `JwtSettings__Secret`
- `MapGeneration__OverpassContactEmail`
- `Fcm__Enabled`
- `Fcm__ServiceAccountJsonPath` или `Fcm__ServiceAccountJson`

Важно:

- по умолчанию приложение в Development пытается загрузить `google-service.json`
- если Firebase не нужен локально, проще выставить `Fcm__Enabled=false`
- если Firebase нужен => service account JSON в путь из `appsettings.json`

### 3. Поднять базу

```bash
docker compose up -d
```

Это поднимет:

- PostgreSQL/PostGIS на `localhost:5432` по умолчанию
- PgAdmin на `localhost:8080` по умолчанию

### 4. Запустить API

```bash
dotnet restore
dotnet run --project RoadSafety-backend.Presentation
```

В Development приложение доступно на:

- `http://localhost:5103`
- `https://localhost:7246`

При старте в Development автоматически применяются миграции к БД.

## API

Базовый префикс: `/api`

### Auth

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`

### Users

- `GET /api/users?email=&phone=`
- `GET /api/users/me`

### Families

- `POST /api/families`
- `GET /api/families/{familyId}`
- `PUT /api/families/{familyId}/city`
- `GET /api/families/{familyId}/members`
- `POST /api/families/join-by-invite`
- `POST /api/families/invite-code`

### Maps

- `GET /api/maps/cities`
- `GET /api/maps/cities/{cityId}/metadata`
- `GET /api/maps/tiles/{cityId}/{z}/{x}/{y}.pbf`
- `GET /api/maps/user-areas?familyId=&childId=`
- `GET /api/maps/alert-zones?cityId=&familyId=&childId=`
- `POST /api/maps/user-areas/base-overrides`
- `POST /api/maps/user-areas/custom`
- `DELETE /api/maps/user-areas/custom/{areaId}`
- `DELETE /api/maps/user-areas/base-overrides?familyId=&baseAreaKey=&childId=`

### Tracking

- `POST /api/tracking/location`
- `GET /api/tracking/children/{childId}/location`
- `GET /api/tracking/children/locations`
- `GET /api/tracking/children/{childId}/stats`

### Notifications

- `GET /api/notifications`
- `POST /api/notifications/{id}/read`
- `POST /api/notifications/device-tokens`
- `DELETE /api/notifications/device-tokens/{token}`

### Документация API

- OpenAPI генерируется в Development
- Scalar UI маппится в Development из `Program.cs`
- полный контракт API также лежит в `RoadSafetyApi.yaml`

## База данных

База работает на PostgreSQL + PostGIS. Основные таблицы описаны в `db.md`.

Ключевые сущности:

- `users`
- `families`
- `family_members`
- `invite_codes`
- `sessions`
- `refresh_tokens`
- `map_areas`
- `user_map_areas`
- `map_city_metadata`
- `child_locations`
- `child_risk_states`
- `child_stats`
- `notifications`
- `device_tokens`

### Миграции

Миграции лежат в:

`RoadSafety-backend.Infrastructure/Persistence/PostgreSQL/Migrations`

Ручная команда для добавления новой миграции:

```bash
dotnet ef migrations add MigrationName --project RoadSafety-backend.Infrastructure --startup-project RoadSafety-backend.Presentation
```