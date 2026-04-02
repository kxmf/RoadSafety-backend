using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoadSafety_backend.Domain.Aggregates;
namespace RoadSafety_backend.Infrastructure.Persistence.Configurations;

public class FamilyConfiguraion : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("family");

        builder.HasKey(x => x.Id);
    }
}
