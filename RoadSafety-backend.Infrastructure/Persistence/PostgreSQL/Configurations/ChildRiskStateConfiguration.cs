using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoadSafety_backend.Domain.Aggregates.TrackingAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class ChildRiskStateConfiguration : IEntityTypeConfiguration<ChildRiskState>
{
    public void Configure(EntityTypeBuilder<ChildRiskState> builder)
    {
        builder.ToTable("child_risk_states");

        builder.HasKey(state => state.ChildId);

        builder.Property(state => state.ChildId)
            .HasColumnName("child_id");

        builder.Property(state => state.CurrentRisk)
            .HasColumnName("current_risk")
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();

        builder.Property(state => state.EnteredRedAt)
            .HasColumnName("entered_red_at");

        builder.Property(state => state.LastRedNotificationAt)
            .HasColumnName("last_red_notification_at");

        builder.Property(state => state.LastUpdatedAt)
            .HasColumnName("last_updated_at");

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(state => state.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
