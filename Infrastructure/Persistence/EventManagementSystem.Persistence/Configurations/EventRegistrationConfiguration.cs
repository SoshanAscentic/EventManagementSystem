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

            // Value Object configurations
            builder.OwnsOne(r => r.EventId, eid =>
            {
                eid.Property(x => x.Value)
                    .HasColumnName("EventId")
                    .IsRequired();
            });

            builder.OwnsOne(r => r.UserId, uid =>
            {
                uid.Property(x => x.Value)
                    .HasColumnName("UserId")
                    .IsRequired();
            });

            builder.OwnsOne(r => r.Status, s =>
            {
                s.Property(x => x.Value)
                    .HasColumnName("Status")
                    .HasMaxLength(20)
                    .IsRequired();
            });

            // Properties
            builder.Property(r => r.RegisteredAt)
                .IsRequired();

            builder.Property(r => r.CancelledAt);

            builder.Property(r => r.Notes)
                .HasMaxLength(1000);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            builder.Property(r => r.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey("EventId")
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.User)
                .WithMany(u => u.Registrations)
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex("EventId")
                .HasDatabaseName("IX_EventRegistrations_EventId");

            builder.HasIndex("UserId")
                .HasDatabaseName("IX_EventRegistrations_UserId");

            builder.HasIndex(r => r.Status)
                .HasDatabaseName("IX_EventRegistrations_Status");

            builder.HasIndex(r => r.RegisteredAt)
                .HasDatabaseName("IX_EventRegistrations_RegisteredAt");

            builder.HasIndex("EventId", "UserId")
                .IsUnique()
                .HasDatabaseName("IX_EventRegistrations_EventId_UserId")
                .HasFilter("[Status] = 'Registered'");

            // Ignore computed properties
            builder.Ignore(r => r.RegistrationId);
            builder.Ignore(r => r.IsActive);
            builder.Ignore(r => r.IsCancelled);
            builder.Ignore(r => r.RegistrationDuration);
            builder.Ignore(r => r.DomainEvents);
        }
    }
}
