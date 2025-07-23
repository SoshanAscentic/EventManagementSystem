// <copyright file="EventConfiguration.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Configurations
{
    using EventManagementSystem.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventConfiguration : IEntityTypeConfiguration<Event>
    {
        public void Configure(EntityTypeBuilder<Event> builder)
        {
            // Table configuration
            builder.ToTable("Events");

            // Primary key
            builder.HasKey(e => e.Id);

            // Basic properties
            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(e => e.CategoryId)
                .IsRequired();

            // Map the backing fields directly - FIXED: Remove quotes
            builder.Property("_startDateTime")
                .HasColumnName("StartDateTime")
                .IsRequired();

            builder.Property("_endDateTime")
                .HasColumnName("EndDateTime")
                .IsRequired();

            builder.Property("_venue")
                .HasColumnName("Venue")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property("_address")
                .HasColumnName("Address")
                .IsRequired()
                .HasMaxLength(200);

            builder.Property("_city")
                .HasColumnName("City")
                .HasMaxLength(100);

            builder.Property("_country")
                .HasColumnName("Country")
                .HasMaxLength(100);

            builder.Property("_capacity")
                .HasColumnName("Capacity")
                .IsRequired();

            builder.Property("_eventType")
                .HasColumnName("EventType")
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Registrations)
                .WithOne(r => r.Event)
                .HasForeignKey("_eventId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Images)
                .WithOne(i => i.Event)
                .HasForeignKey("_eventId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes - use backing field names for queryable columns
            builder.HasIndex(e => e.Title)
                .HasDatabaseName("IX_Events_Title");

            builder.HasIndex("_startDateTime")
                .HasDatabaseName("IX_Events_StartDateTime");

            builder.HasIndex(e => e.CategoryId)
                .HasDatabaseName("IX_Events_CategoryId");

            // Ignore computed properties and value objects - these cannot be queried directly
            builder.Ignore(e => e.EventId);
            builder.Ignore(e => e.EventDateTime);
            builder.Ignore(e => e.Location);
            builder.Ignore(e => e.Capacity);
            builder.Ignore(e => e.EventType);
            builder.Ignore(e => e.CurrentRegistrations);
            builder.Ignore(e => e.RemainingCapacity);
            builder.Ignore(e => e.IsFull);
            builder.Ignore(e => e.IsRegistrationOpen);
            builder.Ignore(e => e.IsUpcoming);
            builder.Ignore(e => e.IsOngoing);
            builder.Ignore(e => e.IsCompleted);
            builder.Ignore(e => e.PrimaryImage);
            builder.Ignore(e => e.DomainEvents);
        }
    }
}
