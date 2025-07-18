// <copyright file="EventRegistrationConfiguration.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistance.Configuration
{
    using EventManagementSystem.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class EventRegistrationConfiguration : IEntityTypeConfiguration<EventRegistration>
    {
        public void Configure(EntityTypeBuilder<EventRegistration> builder)
        {
            builder.ToTable("EventRegistrations");

            // Primary Key
            builder.HasKey(r => r.Id);

            // Value Objects
            builder.OwnsOne(r => r.EventId, eid =>
            {
                eid.Property(p => p.Value)
                    .IsRequired()
                    .HasColumnName("EventId");
            });

            builder.OwnsOne(r => r.UserId, uid =>
            {
                uid.Property(p => p.Value)
                    .IsRequired()
                    .HasColumnName("UserId");
            });

            builder.OwnsOne(r => r.Status, s =>
            {
                s.Property(p => p.Value)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("Status")
                    .HasDefaultValue("Registered");
            });

            // Properties
            builder.Property(r => r.RegisteredAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(r => r.CancelledAt)
                .IsRequired(false);

            builder.Property(r => r.Notes)
                .HasMaxLength(1000);

            // Relationships are configured via shadow properties since we're using value objects
            builder.HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey("EventId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                .WithMany(u => u.Registrations)
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex("EventId")
                .HasDatabaseName("IX_EventRegistrations_EventId");

            builder.HasIndex("UserId")
                .HasDatabaseName("IX_EventRegistrations_UserId");

            builder.HasIndex("Status")
                .HasDatabaseName("IX_EventRegistrations_Status");

            builder.HasIndex(r => r.RegisteredAt)
                .HasDatabaseName("IX_EventRegistrations_RegisteredAt");

            // Unique constraint: one active registration per user per event
            builder.HasIndex("UserId", "EventId", "Status")
                .HasDatabaseName("IX_EventRegistrations_User_Event_Status");

            // Composite indexes for common queries
            builder.HasIndex("EventId", "Status")
                .HasDatabaseName("IX_EventRegistrations_Event_Status");

            builder.HasIndex("UserId", "Status")
                .HasDatabaseName("IX_EventRegistrations_User_Status");

            // Audit fields
            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(r => r.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Ignore computed properties
            builder.Ignore(r => r.RegistrationId);
            builder.Ignore(r => r.IsActive);
            builder.Ignore(r => r.IsCancelled);
            builder.Ignore(r => r.RegistrationDuration);
            builder.Ignore(r => r.DomainEvents);
        }
    }
}
