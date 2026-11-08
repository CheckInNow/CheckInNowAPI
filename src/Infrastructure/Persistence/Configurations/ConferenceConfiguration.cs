using System;
using System.Collections.Generic;
using System.Text;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public sealed class ConferenceConfiguration : IEntityTypeConfiguration<Conference>
{
    public void Configure(EntityTypeBuilder<Conference> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(1000);
        builder.Property(c => c.Location)
            .IsRequired()
            .HasMaxLength(200);
        builder.Property(c => c.ParticipantLimit)
            .IsRequired();

        builder.HasOne(c => c.Organizer)
            .WithMany(u => u.Conferences)
            .HasForeignKey(c => c.OrganizerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
