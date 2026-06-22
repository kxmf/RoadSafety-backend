using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoadSafety_backend.Domain.Aggregates.DeviceTokenAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("device_tokens");

        builder.HasKey(deviceToken => deviceToken.Id);

        builder.Property(deviceToken => deviceToken.Id)
            .HasColumnName("id");

        builder.Property(deviceToken => deviceToken.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(deviceToken => deviceToken.Token)
            .HasColumnName("token")
            .HasMaxLength(4096)
            .IsRequired();

        builder.Property(deviceToken => deviceToken.Platform)
            .HasColumnName("platform")
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();

        builder.Property(deviceToken => deviceToken.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(deviceToken => deviceToken.LastSeenAt)
            .HasColumnName("last_seen_at");

        builder.Property(deviceToken => deviceToken.RevokedAt)
            .HasColumnName("revoked_at");

        builder.HasIndex(deviceToken => deviceToken.Token)
            .IsUnique()
            .HasFilter("revoked_at IS NULL");

        builder.HasIndex(deviceToken => deviceToken.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(deviceToken => deviceToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
