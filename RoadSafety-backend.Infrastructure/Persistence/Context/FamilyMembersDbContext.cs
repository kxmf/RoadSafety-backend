using Microsoft.EntityFrameworkCore;
using RoadSafety_backend.Domain.Entities;
using System.Reflection;

namespace RoadSafety_backend.Infrastructure.Persistence.Context;

public class FamilyMembersDbContext : DbContext
{
    public FamilyMembersDbContext(DbContextOptions<FamilyMembersDbContext> options)
        : base(options)
    {
    }

    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
