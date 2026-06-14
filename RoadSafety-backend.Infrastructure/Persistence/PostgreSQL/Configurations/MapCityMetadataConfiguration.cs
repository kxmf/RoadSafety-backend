using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoadSafety_backend.Domain.Aggregates.MapAggregate;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class MapCityMetadataConfiguration : IEntityTypeConfiguration<MapCityMetadata>
{
    public void Configure(EntityTypeBuilder<MapCityMetadata> builder)
    {
        builder.ToTable("map_city_metadata");

        builder.HasKey(metadata => metadata.CityId);

        builder.Property(metadata => metadata.CityId)
            .HasColumnName("city_id")
            .HasColumnType("varchar(50)");

        builder.Property(metadata => metadata.GenerationVersion)
            .HasColumnName("generation_version")
            .IsRequired();

        builder.Property(metadata => metadata.MinLon)
            .HasColumnName("min_lon")
            .IsRequired();

        builder.Property(metadata => metadata.MinLat)
            .HasColumnName("min_lat")
            .IsRequired();

        builder.Property(metadata => metadata.MaxLon)
            .HasColumnName("max_lon")
            .IsRequired();

        builder.Property(metadata => metadata.MaxLat)
            .HasColumnName("max_lat")
            .IsRequired();
    }
}
