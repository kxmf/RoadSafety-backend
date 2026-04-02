using Microsoft.EntityFrameworkCore;
using RoadSafety_backend.Domain.Aggregates;
using RoadSafety_backend.Domain.Entities;
using System.Reflection;

namespace RoadSafety_backend.Infrastructure.Persistence.Context;

public class FamilyDbContext : DbContext
{
    public FamilyDbContext(DbContextOptions<FamilyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Family> Families => Set<Family>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
