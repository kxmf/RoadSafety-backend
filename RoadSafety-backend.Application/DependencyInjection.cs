using Microsoft.Extensions.DependencyInjection;
using RoadSafety_backend.Application.UseCases.Auth;
using RoadSafety_backend.Application.UseCases.Family;
using RoadSafety_backend.Application.UseCases.Maps;
using RoadSafety_backend.Application.UseCases.Notifications;
using RoadSafety_backend.Application.UseCases.Tracking;
using RoadSafety_backend.Application.UseCases.Users;

namespace RoadSafety_backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginUseCase>();
        services.AddScoped<LogOutUseCase>();
        services.AddScoped<RefreshTokensUseCase>();
        services.AddScoped<RegisterUseCase>();
        services.AddScoped<CreateFamilyUseCase>();
        services.AddScoped<GetFamilyMembersUseCase>();
        services.AddScoped<JoinFamilyByInviteCodeUseCase>();
        services.AddScoped<CreateInviteCodeUseCase>();
        services.AddScoped<UpdateFamilyCityUseCase>();
        services.AddScoped<GetUserByContactUseCase>();
        services.AddScoped<GetCurrentUserUseCase>();
        services.AddScoped<GetUserMapAreasUseCase>();
        services.AddScoped<GetMapCitiesUseCase>();
        services.AddScoped<GetMapTileUseCase>();
        services.AddScoped<GetCityMetadataUseCase>();
        services.AddScoped<GetAlertZonesUseCase>();
        services.AddScoped<CreateBaseAreaOverrideUseCase>();
        services.AddScoped<CreateCustomUserMapAreaUseCase>();
        services.AddScoped<DeleteBaseAreaOverrideUseCase>();
        services.AddScoped<DeleteCustomUserMapAreaUseCase>();
        services.AddScoped<SubmitLocationUseCase>();
        services.AddScoped<GetChildLocationUseCase>();
        services.AddScoped<GetChildrenLocationsUseCase>();
        services.AddScoped<GetChildStatsUseCase>();
        services.AddScoped<GetNotificationsUseCase>();
        services.AddScoped<MarkNotificationReadUseCase>();
        services.AddScoped<RegisterDeviceTokenUseCase>();
        services.AddScoped<DeleteDeviceTokenUseCase>();

        return services;
    }
}
