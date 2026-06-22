using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoadSafety_backend.Domain.Aggregates.TrackingAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class ChildStatsConfiguration : IEntityTypeConfiguration<ChildStats>
{
    public void Configure(EntityTypeBuilder<ChildStats> builder)
    {
        builder.ToTable("child_stats");

        builder.HasKey(stats => stats.ChildId);

        builder.Property(stats => stats.ChildId)
            .HasColumnName("child_id");

        builder.Property(stats => stats.TotalScore)
            .HasColumnName("total_score");

        builder.Property(stats => stats.Rating)
            .HasColumnName("rating");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(stats => stats.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
