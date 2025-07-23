// <copyright file="EventImageConfiguration.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Configurations
{
    using EventManagementSystem.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventImageConfiguration : IEntityTypeConfiguration<EventImage>
    {
        public void Configure(EntityTypeBuilder<EventImage> builder)
        {
            // Table configuration
            builder.ToTable("EventImages");

            // Primary key
            builder.HasKey(i => i.Id);

            // Map backing field directly
            builder.Property<int>("_eventId")
                .HasColumnName("EventId")
                .IsRequired();

            // Basic properties
            builder.Property(i => i.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(i => i.FilePath)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(i => i.FileSize)
                .IsRequired();

            builder.Property(i => i.IsPrimary)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(i => i.UploadedAt)
                .IsRequired();

            builder.Property(i => i.CreatedAt)
                .IsRequired();

            builder.Property(i => i.UpdatedAt)
                .IsRequired();

            // Relationships using backing field
            builder.HasOne(i => i.Event)
                .WithMany(e => e.Images)
                .HasForeignKey("_eventId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex("_eventId")
                .HasDatabaseName("IX_EventImages_EventId");

            builder.HasIndex(i => i.IsPrimary)
                .HasDatabaseName("IX_EventImages_IsPrimary");

            // Constraint - Only one primary image per event
            builder.HasIndex("_eventId", nameof(EventImage.IsPrimary))
                .IsUnique()
                .HasDatabaseName("IX_EventImages_EventId_IsPrimary_Unique")
                .HasFilter("[IsPrimary] = 1");

            // Ignore value object property
            builder.Ignore(i => i.EventId);
        }
    }
}
