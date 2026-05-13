using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique();

        builder.Property(rt => rt.SessionId)
            .IsRequired();

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();

        builder.Property(rt => rt.IsUsed)
            .HasDefaultValue(false);

        builder.Property(rt => rt.IsRevoked)
            .HasDefaultValue(false);

        builder.HasOne<RefreshToken>()
            .WithOne()
            .HasForeignKey<RefreshToken>(rt => rt.ReplacedByTokenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(t => t.IsExpired);
        builder.Ignore(t => t.IsActive);
    }
}

public class RefreshTokenIdConverter : ValueConverter<RefreshTokenId, Guid>
{
    public RefreshTokenIdConverter() : base(id => id.Id, value => new RefreshTokenId(value)) { }
}
