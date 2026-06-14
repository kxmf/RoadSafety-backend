using Microsoft.Extensions.DependencyInjection;
using RoadSafety_backend.Application.UseCases.Auth;
using RoadSafety_backend.Application.UseCases.Family;
using RoadSafety_backend.Application.UseCases.Maps;
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
        services.AddScoped<CreateBaseAreaOverrideUseCase>();
        services.AddScoped<CreateCustomUserMapAreaUseCase>();

        return services;
    }
}
