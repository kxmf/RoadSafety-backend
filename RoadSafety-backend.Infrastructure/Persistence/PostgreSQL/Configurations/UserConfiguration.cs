using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RoadSafety_backend.Domain.Aggregates.FamilyAggregate;
using RoadSafety_backend.Domain.Aggregates.UserAggregate;
using System.Net.Mail;

namespace RoadSafety_backend.Infrastructure.Persistence.PostgreSQL.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.OwnsOne(u => u.Profile, profile =>
        {
            profile.Property(p => p.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100);

            profile.Property(p => p.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100);

            profile.Property(p => p.Patronymic)
                .HasColumnName("patronymic")
                .HasMaxLength(100);

            profile.Property(p => p.BirthDate)
                .HasColumnType("date")
                .HasColumnName("birth_date");
        });

        builder.Property(u => u.HashedPassword)
            .HasColumnName("hashed_password")
            .HasMaxLength(255);

        builder.OwnsOne(u => u.Contacts, contacts =>
        {
            contacts.Property(c => c.MailAddress)
                .HasColumnName("mail_address")
                .HasConversion(
                    v => v.ToString(),
                    v => new MailAddress(v))
                .HasColumnType("citext")
                .HasMaxLength(255);

            contacts.Property(c => c.PhoneNumber)
                .HasColumnName("phone_number")
                .HasConversion(
                    v => v.ToString(),
                    v => new PhoneNumber(v))
                .HasMaxLength(40);

            contacts.HasIndex(c => c.MailAddress).IsUnique();
            contacts.HasIndex(c => c.PhoneNumber).IsUnique();
        });

        builder.Property(u => u.Role)
            .HasColumnName("role")
            .HasConversion(
            r => r.ToString(),
            r => Enum.Parse<UserRole>(r))
            .HasColumnType("citext");

        builder.HasOne<Family>()
               .WithMany()
               .HasForeignKey(u => u.FamilyId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UserIdConverter : ValueConverter<UserId, Guid>
{
    public UserIdConverter() : base(id => id.Id, value => new UserId(value)) { }
}