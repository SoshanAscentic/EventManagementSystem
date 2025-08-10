// <copyright file="ApplicationDbContext.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Context
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Persistence.Configurations;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; } = null!;

        public DbSet<EventCategory> EventCategories { get; set; } = null!;

        public DbSet<EventImage> EventImages { get; set; } = null!;

        public DbSet<EventRegistration> EventRegistrations { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Apply audit information before saving
                this.ApplyAuditInformation();

                var result = await base.SaveChangesAsync(cancellationToken);
                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("The data was modified by another process. Please refresh and try again.", ex);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations
            modelBuilder.ApplyConfiguration(new EventConfiguration());
            modelBuilder.ApplyConfiguration(new EventCategoryConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new EventRegistrationConfiguration());
            modelBuilder.ApplyConfiguration(new EventImageConfiguration());

            // Apply any additional constraints
            this.ConfigureAdditionalConstraints(modelBuilder);
        }

        private void ConfigureAdditionalConstraints(ModelBuilder modelBuilder)
        {
            // Ensure event dates are logical - using the correct column names
            modelBuilder.Entity<Event>()
                .HasCheckConstraint("CK_Events_DateRange", "[EndDateTime] > [StartDateTime]");

            // Ensure capacity is positive - using the correct column name
            modelBuilder.Entity<Event>()
                .HasCheckConstraint("CK_Events_PositiveCapacity", "[Capacity] > 0");

            // Ensure file size is positive
            modelBuilder.Entity<EventImage>()
                .HasCheckConstraint("CK_EventImages_PositiveFileSize", "[FileSize] > 0");
        }

        private void ApplyAuditInformation()
        {
            var entries = this.ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.MarkAsUpdated();
                }
            }
        }
    }
}
