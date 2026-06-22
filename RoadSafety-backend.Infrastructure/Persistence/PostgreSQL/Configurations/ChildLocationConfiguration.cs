using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.TrackingAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class ChildLocationConfiguration : IEntityTypeConfiguration<ChildLocation>
{
    public void Configure(EntityTypeBuilder<ChildLocation> builder)
    {
        builder.ToTable("child_locations");

        builder.HasKey(location => location.ChildId);

        builder.Property(location => location.ChildId)
            .HasColumnName("child_id");

        builder.Property(location => location.FamilyId)
            .HasColumnName("family_id");

        builder.Property(location => location.Location)
            .HasColumnName("location")
            .HasColumnType("geometry(Point, 4326)")
            .IsRequired();

        builder.Property(location => location.AccuracyMeters)
            .HasColumnName("accuracy_meters");

        builder.Property(location => location.CurrentRisk)
            .HasColumnName("current_risk")
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();

        builder.Property(location => location.MatchedUserAreaId)
            .HasColumnName("matched_user_area_id");

        builder.Property(location => location.MatchedBaseAreaKey)
            .HasColumnName("matched_base_area_key")
            .HasColumnType("varchar");

        builder.Property(location => location.RecordedAt)
            .HasColumnName("recorded_at");

        builder.Property(location => location.LastUpdatedAt)
            .HasColumnName("last_updated_at");

        builder.HasIndex(location => location.FamilyId);
        builder.HasIndex(location => location.Location)
            .HasDatabaseName("child_locations_location_idx")
            .HasMethod("gist");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(location => location.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Family>()
            .WithMany()
            .HasForeignKey(location => location.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
