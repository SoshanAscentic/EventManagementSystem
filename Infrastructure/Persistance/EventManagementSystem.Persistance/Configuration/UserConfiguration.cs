// <copyright file="UserConfiguration.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistance.Configuration
{
    using EventManagementSystem.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            // Primary Key
            builder.HasKey(u => u.Id);

            // Value Object: Email
            builder.OwnsOne(u => u.Email, e =>
            {
                e.Property(p => p.Value)
                    .IsRequired()
                    .HasMaxLength(254)
                    .HasColumnName("Email");
            });

            // Properties
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            // Value Object: Phone (nullable)
            builder.OwnsOne(u => u.Phone, p =>
            {
                p.Property(ph => ph.Value)
                    .HasMaxLength(20)
                    .HasColumnName("Phone");
            });

            // Relationships
            builder.HasMany(u => u.Registrations)
                .WithOne(r => r.User)
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex("Email")
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(u => u.FirstName)
                .HasDatabaseName("IX_Users_FirstName");

            builder.HasIndex(u => u.LastName)
                .HasDatabaseName("IX_Users_LastName");

            builder.HasIndex(u => new { u.FirstName, u.LastName })
                .HasDatabaseName("IX_Users_FullName");

            // Audit fields
            builder.Property(u => u.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(u => u.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Ignore computed properties
            builder.Ignore(u => u.UserId);
            builder.Ignore(u => u.FullName);
            builder.Ignore(u => u.ActiveRegistrationsCount);
            builder.Ignore(u => u.DomainEvents);
        }
    }
}
