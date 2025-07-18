// <copyright file="EventConfiguration.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistance.Configuration
{
    using EventManagementSystem.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            builder.ToTable("Events");

            // Primary Key
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(2000);

            // Value Object: EventDateTime
            builder.OwnsOne(e => e.EventDateTime, ed =>
            {
                ed.Property(p => p.StartDateTime)
                    .IsRequired()
                    .HasColumnName("StartDateTime");

                ed.Property(p => p.EndDateTime)
                    .IsRequired()
                    .HasColumnName("EndDateTime");
            });

            // Value Object: EventLocation
            builder.OwnsOne(e => e.Location, el =>
            {
                el.Property(p => p.Venue)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("Venue");

                el.Property(p => p.Address)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("Address");

                el.Property(p => p.City)
                    .HasMaxLength(100)
                    .HasColumnName("City");

                el.Property(p => p.Country)
                    .HasMaxLength(100)
                    .HasColumnName("Country");
            });

            // Value Object: EventCapacity
            builder.OwnsOne(e => e.Capacity, ec =>
            {
                ec.Property(p => p.Value)
                    .IsRequired()
                    .HasColumnName("Capacity");
            });

            // Value Object: EventType
            builder.OwnsOne(e => e.EventType, et =>
            {
                et.Property(p => p.Value)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("EventType");
            });

            // Foreign Keys
            builder.Property(e => e.CategoryId)
                .IsRequired();

            // Relationships
            builder.HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Registrations)
                .WithOne(r => r.Event)
                .HasForeignKey("EventId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Images)
                .WithOne(i => i.Event)
                .HasForeignKey("EventId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(e => e.CategoryId)
                .HasDatabaseName("IX_Events_CategoryId");

            builder.HasIndex(e => new { e.Title })
                .HasDatabaseName("IX_Events_Title");

            builder.HasIndex("StartDateTime")
                .HasDatabaseName("IX_Events_StartDateTime");

            builder.HasIndex("EndDateTime")
                .HasDatabaseName("IX_Events_EndDateTime");

            builder.HasIndex("Venue")
                .HasDatabaseName("IX_Events_Venue");

            builder.HasIndex("City")
                .HasDatabaseName("IX_Events_City");

            // Composite indexes for common queries
            builder.HasIndex("StartDateTime", "CategoryId")
                .HasDatabaseName("IX_Events_StartDateTime_CategoryId");

            builder.HasIndex("EventType", "StartDateTime")
                .HasDatabaseName("IX_Events_EventType_StartDateTime");

            // Audit fields from BaseEntity
            builder.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Ignore navigation to value objects that shouldn't be persisted
            builder.Ignore(e => e.EventId);
            builder.Ignore(e => e.CurrentRegistrations);
            builder.Ignore(e => e.IsFull);
            builder.Ignore(e => e.RemainingCapacity);
            builder.Ignore(e => e.IsRegistrationOpen);
            builder.Ignore(e => e.IsUpcoming);
            builder.Ignore(e => e.IsOngoing);
            builder.Ignore(e => e.IsCompleted);
            builder.Ignore(e => e.PrimaryImage);
            builder.Ignore(e => e.DomainEvents);
        }
    }
}
