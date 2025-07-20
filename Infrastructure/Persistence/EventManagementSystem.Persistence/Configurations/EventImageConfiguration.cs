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

            // Value Object configuration
            builder.OwnsOne(i => i.EventId, eid =>
            {
                eid.Property(x => x.Value)
                    .HasColumnName("EventId")
                    .IsRequired();
            });

            // Properties
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

            // Relationships
            builder.HasOne(i => i.Event)
                .WithMany(e => e.Images)
                .HasForeignKey("EventId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex("EventId")
                .HasDatabaseName("IX_EventImages_EventId");

            builder.HasIndex(i => i.IsPrimary)
                .HasDatabaseName("IX_EventImages_IsPrimary");

            // Constraints - Only one primary image per event
            builder.HasIndex("EventId", i => i.IsPrimary)
                .IsUnique()
                .HasDatabaseName("IX_EventImages_EventId_IsPrimary")
                .HasFilter("[IsPrimary] = 1");
        }
    }
}
