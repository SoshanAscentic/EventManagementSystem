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

            // Properties
            builder.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .IsRequired();

            // Value Object configurations
            builder.OwnsOne(e => e.EventDateTime, dt =>
            {
                dt.Property(x => x.StartDateTime)
                    .HasColumnName("StartDateTime")
                    .IsRequired();

                dt.Property(x => x.EndDateTime)
                    .HasColumnName("EndDateTime")
                    .IsRequired();
            });

            builder.OwnsOne(e => e.Location, loc =>
            {
                loc.Property(x => x.Venue)
                    .HasColumnName("Venue")
                    .HasMaxLength(100)
                    .IsRequired();

                loc.Property(x => x.Address)
                    .HasColumnName("Address")
                    .HasMaxLength(200)
                    .IsRequired();

                loc.Property(x => x.City)
                    .HasColumnName("City")
                    .HasMaxLength(50);

                loc.Property(x => x.Country)
                    .HasColumnName("Country")
                    .HasMaxLength(50);
            });

            builder.OwnsOne(e => e.Capacity, cap =>
            {
                cap.Property(x => x.Value)
                    .HasColumnName("Capacity")
                    .IsRequired();
            });

            builder.OwnsOne(e => e.EventType, et =>
            {
                et.Property(x => x.Value)
                    .HasColumnName("EventType")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // Foreign keys
            builder.Property(e => e.CategoryId)
                .IsRequired();

            // Relationships
            builder.HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(e => e.Registrations)
                .WithOne(r => r.Event)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.Images)
                .WithOne(i => i.Event)
                .HasForeignKey(i => i.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            builder.HasIndex(e => e.CategoryId)
                .HasDatabaseName("IX_Events_CategoryId");

            builder.HasIndex(e => new { e.Title, e.StartDateTime })
                .HasDatabaseName("IX_Events_Title_StartDateTime");

            builder.HasIndex(e => e.StartDateTime)
                .HasDatabaseName("IX_Events_StartDateTime");

            builder.HasIndex(e => e.EndDateTime)
                .HasDatabaseName("IX_Events_EndDateTime");

            // Computed columns
            builder.Property<int>("CurrentRegistrations")
                .HasComputedColumnSql("(SELECT COUNT(*) FROM EventRegistrations WHERE EventId = Events.Id AND Status = 'Registered')", stored: false);

            // Ignore domain events (they shouldn't be persisted)
            builder.Ignore(e => e.DomainEvents);
            builder.Ignore(e => e.EventId);
            builder.Ignore(e => e.CurrentRegistrations);
            builder.Ignore(e => e.IsFull);
            builder.Ignore(e => e.RemainingCapacity);
            builder.Ignore(e => e.IsRegistrationOpen);
            builder.Ignore(e => e.IsUpcoming);
            builder.Ignore(e => e.IsOngoing);
            builder.Ignore(e => e.IsCompleted);
            builder.Ignore(e => e.PrimaryImage);
        }
    }
}
