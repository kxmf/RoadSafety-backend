using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class MapAreaConfiguration : IEntityTypeConfiguration<MapArea>
{
    public void Configure(EntityTypeBuilder<MapArea> builder)
    {
        builder.ToTable("map_areas");

        builder.HasKey(area => area.Id);

        builder.Property(area => area.Id)
            .HasColumnName("id");

        builder.Property(area => area.OsmId)
            .HasColumnName("osm_id");

        builder.Property(area => area.BaseAreaKey)
            .HasColumnName("base_area_key")
            .HasColumnType("varchar")
            .IsRequired();

        builder.Property(area => area.Risk)
            .HasColumnName("risk")
            .HasConversion<string>()
            .HasColumnType("varchar")
            .IsRequired();

        builder.Property(area => area.Geometry)
            .HasColumnName("geom")
            .HasColumnType("geometry(Polygon, 4326)")
            .IsRequired();

        builder.Property(area => area.CityId)
            .HasColumnName("city_id")
            .HasColumnType("varchar");

        builder.HasIndex(area => area.Geometry)
            .HasDatabaseName("map_areas_geom_idx")
            .HasMethod("gist");

        builder.HasIndex(area => area.CityId);
        builder.HasIndex(area => area.BaseAreaKey).IsUnique();
        builder.HasIndex(area => area.Risk);
    }
}

public class MapAreaIdConverter : ValueConverter<MapAreaId, Guid>
{
    public MapAreaIdConverter() : base(id => id.Value, value => new MapAreaId(value)) { }
}
