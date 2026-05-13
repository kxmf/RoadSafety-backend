using Microsoft.Extensions.DependencyInjection;
using RoadSafety_backend.Application.UseCases.Auth;

namespace RoadSafety_backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginUseCase>();
        services.AddScoped<LogOutUseCase>();
        services.AddScoped<RefreshTokensUseCase>();
        services.AddScoped<RegisterUseCase>();

        return services;
    }
}
