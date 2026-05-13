using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadSafety_backend.Domain.Aggregates.SessionAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.IsRevoked)
            .HasDefaultValue(false);

        builder.Metadata.FindNavigation(nameof(Session.RefreshTokens))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.RefreshTokens)
            .WithOne()
            .HasForeignKey(s => s.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.UserId);

        builder.Ignore(s => s.CurrentRefreshToken);
    }
}

public class SessionIdConverter : ValueConverter<SessionId, Guid>
{
    public SessionIdConverter() : base(id => id.Id, value => new SessionId(value)) { }
}
