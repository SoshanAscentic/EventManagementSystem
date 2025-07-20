// <copyright file="ApplicationDbContext.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Context
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly IDomainEventDispatcher domainEventDispatcher;
        private readonly ICurrentUserService currentUserService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IDomainEventDispatcher domainEventDispatcher,
            ICurrentUserService currentUserService)
            : base(options)
        {
            this.domainEventDispatcher = domainEventDispatcher;
            this.currentUserService = currentUserService;
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
                // Handle domain events before saving
                await this.DispatchDomainEventsAsync(cancellationToken);

                // Apply audit information
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

            // Apply all entity configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Apply performance optimizations
            modelBuilder.ApplyPerformanceOptimizations();

            // Apply naming conventions
            modelBuilder.ApplyNamingConventions();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Add interceptors
                optionsBuilder.AddInterceptors(
                    new DomainEventInterceptor(this.domainEventDispatcher),
                    new AuditInterceptor(this.currentUserService));
            }
        }

        private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
        {
            var aggregateRoots = this.ChangeTracker.Entries<IAggregateRoot>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            var domainEvents = aggregateRoots
                .SelectMany(ar => ar.DomainEvents)
                .ToList();

            // Clear domain events before dispatching to prevent infinite loops
            aggregateRoots.ForEach(ar => ar.ClearDomainEvents());

            // Dispatch all domain events
            foreach (var domainEvent in domainEvents)
            {
                await this.domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);
            }
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
