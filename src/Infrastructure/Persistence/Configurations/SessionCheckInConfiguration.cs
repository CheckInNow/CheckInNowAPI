using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class SessionCheckInConfiguration : IEntityTypeConfiguration<SessionCheckIn>
{
    public void Configure(EntityTypeBuilder<SessionCheckIn> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.RegistrationId, x.SessionId })
            .IsUnique();

        builder.HasOne(c => c.Session)
            .WithMany(s => s.SessionCheckIns)
            .HasForeignKey(c => c.SessionId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(c => c.Registration)
            .WithMany(r => r.SessionCheckIns)
            .HasForeignKey(c => c.RegistrationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
