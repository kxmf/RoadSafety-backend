using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class UserMapAreaConfiguration : IEntityTypeConfiguration<UserMapArea>
{
    public void Configure(EntityTypeBuilder<UserMapArea> builder)
    {
        builder.ToTable("user_map_areas");

        builder.HasKey(area => area.Id);

        builder.Property(area => area.Id)
            .HasColumnName("id");

        builder.Property(area => area.FamilyId)
            .HasColumnName("family_id");

        builder.Property(area => area.ChildId)
            .HasColumnName("child_id");

        builder.Property(area => area.BaseAreaKey)
            .HasColumnName("base_area_key")
            .HasColumnType("varchar");

        builder.Property(area => area.Risk)
            .HasColumnName("risk")
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();

        builder.Property(area => area.Geometry)
            .HasColumnName("geom")
            .HasColumnType("geometry(Polygon, 4326)");

        builder.Property(area => area.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(area => area.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("now() at time zone 'utc'");

        builder.Property(area => area.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("now() at time zone 'utc'");

        builder.HasIndex(area => area.Geometry)
            .HasDatabaseName("user_areas_geom_idx")
            .HasMethod("gist");

        builder.HasIndex(area => new { area.FamilyId, area.ChildId });
        builder.HasIndex(area => area.BaseAreaKey);

        builder.HasOne<Family>()
            .WithMany()
            .HasForeignKey(area => area.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(area => area.ChildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(area => area.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserMapAreaIdConverter : ValueConverter<UserMapAreaId, Guid>
{
    public UserMapAreaIdConverter() : base(id => id.Value, value => new UserMapAreaId(value)) { }
}
