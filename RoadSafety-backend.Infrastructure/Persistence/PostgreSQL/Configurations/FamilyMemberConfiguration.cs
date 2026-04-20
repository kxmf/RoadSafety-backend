using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.ToTable("family_members");

        builder.HasKey(x => new { x.UserId, x.FamilyId });

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.FamilyId)
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(x => x.UserRole)
            .HasColumnName("user_role")
            .HasConversion<string>()
            .IsRequired();

        builder.HasOne(x => x.Family)
            .WithMany(f => f.Members)
            .HasForeignKey(x => x.FamilyId);

        builder.HasOne(x => x.User)
            .WithOne(u => u.FamilyMember)
            .HasForeignKey<FamilyMember>(x => x.UserId);
    }
}