using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("families");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.CreatedByUserId)
            .HasColumnName("created_by_user_id");

        builder.Property(f => f.Name)
            .HasColumnName("name");

        builder.Property(f => f.CityId)
            .HasColumnName("city_id")
            .HasColumnType("varchar(50)")
            .IsRequired();

        builder.Metadata.FindNavigation(nameof(Family.Members))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(f => f.Members, familyMemberBuilder =>
        {
            familyMemberBuilder.ToTable("family_members");

            familyMemberBuilder.WithOwner().HasForeignKey("FamilyId");

            familyMemberBuilder.HasKey("FamilyId", nameof(FamilyMember.UserId));

            familyMemberBuilder.Property<DateTime>("joined_at")
                .HasDefaultValueSql("now() at time zone 'utc'");

            familyMemberBuilder.HasOne<User>()
                .WithMany()
                .HasForeignKey(fm => fm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Property<DateTime>("created_at")
            .HasDefaultValueSql("now() at time zone 'utc'");
    }
}

public class FamilyIdConverter : ValueConverter<FamilyId, Guid>
{
    public FamilyIdConverter() : base(id => id.Value, value => new FamilyId(value)) { }
}
