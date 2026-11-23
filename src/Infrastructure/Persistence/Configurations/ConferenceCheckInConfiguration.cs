using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ConferenceCheckInConfiguration : IEntityTypeConfiguration<ConferenceCheckIn>
{
    public void Configure(EntityTypeBuilder<ConferenceCheckIn> builder)
    {
        builder.Property(c => c.RegistrationId)
            .IsRequired();

        builder.Property(c => c.CheckedInAt)
            .IsRequired();

        builder.HasIndex(c => c.RegistrationId).
            IsUnique();

        builder.HasOne(c => c.Registration)
            .WithOne(r => r.ConferenceCheckIn)
            .HasForeignKey<ConferenceCheckIn>(c => c.RegistrationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
