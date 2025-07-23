// <copyright file="EventRegistrationConfiguration.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Configurations
{
    using EventManagementSystem.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
    {
        public void Configure(EntityTypeBuilder<EventRegistration> builder)
        {
            // Table configuration
            builder.ToTable("EventRegistrations");

            // Primary key
            builder.HasKey(r => r.Id);

            // Map backing fields directly - FIXED: Remove quotes
            builder.Property("_eventId")
                .HasColumnName("EventId")
                .IsRequired();

            builder.Property("_userId")
                .HasColumnName("UserId")
                .IsRequired();

            builder.Property("_status")
                .HasColumnName("Status")
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("Registered");

            // Basic properties
            builder.Property(r => r.RegisteredAt)
                .IsRequired();

            builder.Property(r => r.CancelledAt);

            builder.Property(r => r.Notes)
                .HasMaxLength(1000);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.Property(r => r.UpdatedAt)
                .IsRequired();

            // Relationships using backing fields
            builder.HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey("_eventId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                .WithMany(u => u.Registrations)
                .HasForeignKey("_userId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes - use backing field names
            builder.HasIndex("_eventId")
                .HasDatabaseName("IX_EventRegistrations_EventId");

            builder.HasIndex("_userId")
                .HasDatabaseName("IX_EventRegistrations_UserId");

            builder.HasIndex("_status")
                .HasDatabaseName("IX_EventRegistrations_Status");

            builder.HasIndex(r => r.RegisteredAt)
                .HasDatabaseName("IX_EventRegistrations_RegisteredAt");

            // Unique constraint for active registrations
            builder.HasIndex("_eventId", "_userId")
                .IsUnique()
                .HasDatabaseName("IX_EventRegistrations_EventId_UserId_Unique")
                .HasFilter("[Status] = 'Registered'");

            // Ignore computed properties, value objects, and domain events
            builder.Ignore(r => r.RegistrationId);
            builder.Ignore(r => r.EventId);
            builder.Ignore(r => r.UserId);
            builder.Ignore(r => r.Status);
            builder.Ignore(r => r.IsActive);
            builder.Ignore(r => r.IsCancelled);
            builder.Ignore(r => r.RegistrationDuration);
            builder.Ignore(r => r.DomainEvents);
        }
    }
}
