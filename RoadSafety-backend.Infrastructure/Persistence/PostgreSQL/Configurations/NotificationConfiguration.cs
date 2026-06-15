using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoadSafety_backend.Domain.Aggregates.NotificationAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(notification => notification.Id);

        builder.Property(notification => notification.Id)
            .HasColumnName("id");

        builder.Property(notification => notification.RecipientUserId)
            .HasColumnName("recipient_user_id");

        builder.Property(notification => notification.ChildId)
            .HasColumnName("child_id");

        builder.Property(notification => notification.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();

        builder.Property(notification => notification.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(notification => notification.Body)
            .HasColumnName("body")
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(notification => notification.Risk)
            .HasColumnName("risk")
            .HasConversion<string>()
            .HasColumnType("varchar");

        builder.Property(notification => notification.Location)
            .HasColumnName("location")
            .HasColumnType("geometry(Point, 4326)");

        builder.Property(notification => notification.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(notification => notification.ReadAt)
            .HasColumnName("read_at");

        builder.HasIndex(notification => new { notification.RecipientUserId, notification.ReadAt, notification.CreatedAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(notification => notification.RecipientUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(notification => notification.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
