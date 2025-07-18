// <copyright file="EventImageConfiguration.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistance.Configuration
{
    using EventManagementSystem.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventImageConfiguration : IEntityTypeConfiguration<EventImage>
    {
        public void Configure(EntityTypeBuilder<EventImage> builder)
        {
            builder.ToTable("EventImages");

            // Primary Key
            builder.HasKey(i => i.Id);

            // Value Object: EventId
            builder.OwnsOne(i => i.EventId, eid =>
            {
                eid.Property(p => p.Value)
                    .IsRequired()
                    .HasColumnName("EventId");
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
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

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

            builder.HasIndex("EventId", i => i.IsPrimary)
                .HasDatabaseName("IX_EventImages_Event_Primary");

            // Audit fields
            builder.Property(i => i.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(i => i.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Ignore computed properties
            builder.Ignore(i => i.DomainEvents);
        }
    }
}
