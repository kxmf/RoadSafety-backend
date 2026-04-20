using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class FamilyConfiguraion : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("families");

        builder.HasKey(x => x.Id);

        builder.Metadata.FindNavigation(nameof(Family.Members))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Members)
            .WithOne(fm => fm.Family)
            .HasForeignKey(x => x.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FamilyIdConverter : ValueConverter<FamilyId, Guid>
{
    public FamilyIdConverter() : base(id => id.Id, value => new FamilyId(value)) { }
}
