using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Application.UseCases.Tracking;
using RoadSafety_backend.Domain.Aggregates.DeviceTokenAggregate;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.InviteCodeAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.NotificationAggregate;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Aggregates.TrackingAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;
using RoadSafety_backend.Infrastructure.Services;
using RoadSafety_backend.Infrastructure.Services.MapGeneration;
using RoadSafety_backend.Infrastructure.Services.Push;
using RoadSafety_backend.Infrastructure.Services.Settings;

namespace RoadSafety_backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options
                .UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions => npgsqlOptions.UseNetTopologySuite())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll));

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<MapGenerationSettings>(configuration.GetSection("MapGeneration"));
        services.Configure<TrackingOptions>(configuration.GetSection("Tracking"));
        services.Configure<Services.Push.FcmOptions>(configuration.GetSection("Fcm"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IInviteCodeRepository, InviteCodeRepository>();
        services.AddScoped<IMapAreaRepository, MapAreaRepository>();
        services.AddScoped<IMapCityRepository, MapCityRepository>();
        services.AddScoped<IUserMapAreaRepository, UserMapAreaRepository>();
        services.AddScoped<ITrackingRepository, TrackingRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        AddPushNotifications(services, configuration);
        services.AddHttpClient<OverpassMapDataClient>(client =>
        {
            var contactEmail = configuration["MapGeneration:OverpassContactEmail"]?.Trim();
            var userAgent = string.IsNullOrWhiteSpace(contactEmail)
                ? "RoadSafetyBackend/1.0"
                : $"RoadSafetyBackend/1.0 ({contactEmail})";

            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        services.AddScoped<MapAreaGenerationService>();
        services.AddHostedService<MapAreaGenerationBackgroundService>();

        return services;
    }

    private static void AddPushNotifications(IServiceCollection services, IConfiguration configuration)
    {
        var options = new Services.Push.FcmOptions();
        configuration.GetSection("Fcm").Bind(options);

        if (!options.Enabled)
        {
            services.AddScoped<IPushNotificationSender, DisabledPushNotificationSender>();
            return;
        }

        var credential = CreateGoogleCredential(options);
        if (credential is null)
        {
            services.AddScoped<IPushNotificationSender, DisabledPushNotificationSender>();
            return;
        }

        services.AddSingleton(_ =>
        {
            var firebaseOptions = new AppOptions
            {
                Credential = credential,
                ProjectId = string.IsNullOrWhiteSpace(options.ProjectId) ? null : options.ProjectId
            };

            return FirebaseApp.DefaultInstance ?? FirebaseApp.Create(firebaseOptions);
        });
        services.AddSingleton(provider => FirebaseMessaging.GetMessaging(provider.GetRequiredService<FirebaseApp>()));
        services.AddScoped<IPushNotificationSender, FcmPushNotificationSender>();
    }

    private static GoogleCredential? CreateGoogleCredential(Services.Push.FcmOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ServiceAccountJson))
            return CreateServiceAccountCredential(options.ServiceAccountJson, "Fcm:ServiceAccountJson");

        if (!string.IsNullOrWhiteSpace(options.ServiceAccountJsonPath))
        {
            if (!File.Exists(options.ServiceAccountJsonPath))
                throw new InvalidOperationException($"FCM service account JSON file was not found: {options.ServiceAccountJsonPath}");

            return CreateServiceAccountCredential(
                File.ReadAllText(options.ServiceAccountJsonPath),
                "Fcm:ServiceAccountJsonPath");
        }

        return null;
    }

    private static GoogleCredential CreateServiceAccountCredential(string json, string configurationKey)
    {
        try
        {
            return CredentialFactory.FromJson<ServiceAccountCredential>(json).ToGoogleCredential();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException($"{configurationKey} must contain valid JSON.", exception);
        }
    }
}
