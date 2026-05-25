using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoadSafety_backend.Application.Interfaces;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.InviteCodeAggregate;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Repositories;
using RoadSafety_backend.Infrastructure.Services;
using RoadSafety_backend.Infrastructure.Services.Settings;

namespace RoadSafety_backend.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IInviteCodeRepository, InviteCodeRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

        return services;
    }
}
