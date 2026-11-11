using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(r => new { r.ConferenceId, r.Name })
            .IsUnique();

        builder.HasOne(r => r.Conference)
            .WithMany(c => c.Rooms)
            .HasForeignKey(r => r.ConferenceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
