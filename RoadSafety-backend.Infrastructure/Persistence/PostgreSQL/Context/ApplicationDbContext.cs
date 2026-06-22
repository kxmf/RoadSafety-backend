using Microsoft.EntityFrameworkCore;
using RoadSafety_backend.Domain.Aggregates.DeviceTokenAggregate;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.InviteCodeAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.NotificationAggregate;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Aggregates.TrackingAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Context;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Family> Families { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<InviteCode> InviteCodes { get; set; }
    public DbSet<MapArea> MapAreas { get; set; }
    public DbSet<MapCityMetadata> MapCityMetadata { get; set; }
    public DbSet<UserMapArea> UserMapAreas { get; set; }
    public DbSet<ChildLocation> ChildLocations { get; set; }
    public DbSet<ChildStats> ChildStats { get; set; }
    public DbSet<ChildRiskState> ChildRiskStates { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<DeviceToken> DeviceTokens { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<FamilyId>().HaveConversion<FamilyIdConverter>();
        configurationBuilder.Properties<UserId>().HaveConversion<UserIdConverter>();
        configurationBuilder.Properties<SessionId>().HaveConversion<SessionIdConverter>();
        configurationBuilder.Properties<RefreshTokenId>().HaveConversion<RefreshTokenIdConverter>();
        configurationBuilder.Properties<InviteCodeId>().HaveConversion<InviteCodeIdConverter>();
        configurationBuilder.Properties<MapAreaId>().HaveConversion<MapAreaIdConverter>();
        configurationBuilder.Properties<UserMapAreaId>().HaveConversion<UserMapAreaIdConverter>();
    }
}
