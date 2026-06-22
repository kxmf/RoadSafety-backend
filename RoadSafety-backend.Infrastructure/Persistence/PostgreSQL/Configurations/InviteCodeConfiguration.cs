using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.InviteCodeAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class InviteCodeConfiguration : IEntityTypeConfiguration<InviteCode>
{
    public void Configure(EntityTypeBuilder<InviteCode> builder)
    {
        builder.ToTable("invite_codes");

        builder.HasKey(ic => ic.Id);

        builder.Property(ic => ic.Value)
            .HasConversion(
                ivc => ivc.Value,
                value => InviteCodeValue.Create(value))
            .HasMaxLength(6)
            .HasColumnName("value")
            .IsRequired();

        builder.Property(ic => ic.Role)
            .HasColumnName("role")
            .HasConversion(
            fmr => fmr.ToString(),
            fmr => Enum.Parse<FamilyMemberRole>(fmr))
            .HasColumnType("citext");

        builder.Property(ic => ic.FamilyId)
            .HasColumnName("family_id")
            .IsRequired();

        builder.Property(ic => ic.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(ic => ic.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(ic => ic.AcceptedAt)
            .HasColumnName("accepted_at")
            .IsRequired(false);

        builder.Property(ic => ic.IsUsed)
            .HasColumnName("is_used")
            .IsRequired();

        builder.Ignore(ic => ic.IsExpired);
        builder.Ignore(ic => ic.IsActive);

        builder.HasIndex(ic => ic.Value).IsUnique();

        builder.Property<DateTime>("created_at")
            .HasDefaultValueSql("now() at time zone 'utc'");

        builder.HasOne<Family>()
            .WithMany()
            .HasForeignKey(ic => ic.FamilyId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(ic => ic.CreatedByUserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class InviteCodeIdConverter : ValueConverter<InviteCodeId, Guid>
{
    public InviteCodeIdConverter() : base(id => id.Value, value => new InviteCodeId(value)) { }
}