using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title).IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description).IsRequired()
            .HasMaxLength(1000);

        builder.Property(s => s.StartTime)
            .IsRequired();


        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Sessions_StartTime_Before_EndTime",
            "\"StartTime\" < \"EndTime\""));

        builder.Property(s => s.ConferenceId)
            .IsRequired();

        builder.HasOne(s => s.Conference)
            .WithMany(c => c.Sessions)
            .HasForeignKey(s => s.ConferenceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Room)
            .WithMany()
            .HasForeignKey(s => s.RoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Speaker)
            .WithMany()
            .HasForeignKey(s => s.SpeakerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
