// <copyright file="UserConfiguration.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Configurations
{
    using EventManagementSystem.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Table configuration
            builder.ToTable("Users");

            // Primary key
            builder.HasKey(u => u.Id);

            // Basic properties
            builder.Property(u => u.FirstName)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired()
                .HasMaxLength(50);

            // Map backing fields directly instead of using owned types
            builder.Property("_email")
                .HasColumnName("Email")
                .HasMaxLength(254)
                .IsRequired();

            builder.Property("_phone")
                .HasColumnName("Phone")
                .HasMaxLength(20);

            builder.Property(u => u.CreatedAt)
                .IsRequired();

            builder.Property(u => u.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasMany(u => u.Registrations)
                .WithOne(r => r.User)
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex("_email")
                .IsUnique()
                .HasDatabaseName("IX_Users_Email");

            builder.HasIndex(u => new { u.FirstName, u.LastName })
                .HasDatabaseName("IX_Users_FirstName_LastName");

            // Ignore computed properties, value objects, and domain events
            builder.Ignore(u => u.UserId);
            builder.Ignore(u => u.Email);
            builder.Ignore(u => u.Phone);
            builder.Ignore(u => u.FullName);
            builder.Ignore(u => u.ActiveRegistrationsCount);
            builder.Ignore(u => u.DomainEvents);
        }
    }
}
