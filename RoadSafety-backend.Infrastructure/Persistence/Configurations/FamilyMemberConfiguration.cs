using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoadSafety_backend.Domain.Aggregates;
using RoadSafety_backend.Domain.Entities;

namespace RoadSafety_backend.Infrastructure.Persistence.Configurations;

public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.ToTable("family_members");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.FamilyId }).IsUnique();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.UserRole)
            .HasColumnName("user_role")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(x => x.FamilyId)
            .HasColumnName("family_id")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Family>()
            .WithMany()
            .HasForeignKey(x => x.FamilyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}