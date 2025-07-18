// <copyright file="EventManagementDbContext.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistance.Data
{
    using System.Data;
    using System.Reflection;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;

    public class EventManagementDbContext : DbContext, IApplicationDbContext
    {
        private readonly IDomainEventDispatcher? domainEventDispatcher;
        private IDbContextTransaction? currentTransaction;

        public EventManagementDbContext(DbContextOptions<EventManagementDbContext> options)
            : base(options)
        {
        }

        public EventManagementDbContext(
            DbContextOptions<EventManagementDbContext> options,
            IDomainEventDispatcher domainEventDispatcher)
            : base(options)
        {
            this.domainEventDispatcher = domainEventDispatcher;
        }

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Event> Events { get; set; } = null!;

        public DbSet<EventRegistration> EventRegistrations { get; set; } = null!;

        public DbSet<EventCategory> EventCategories { get; set; } = null!;

        public DbSet<EventImage> EventImages { get; set; } = null!;

        public bool HasActiveTransaction => this.currentTransaction != null;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Update audit fields
            this.UpdateAuditFields();

            // Dispatch domain events before saving
            await this.DispatchDomainEventsAsync(cancellationToken);

            // Save changes
            var result = await base.SaveChangesAsync(cancellationToken);

            return result;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (this.currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            this.currentTransaction = await this.Database.BeginTransactionAsync(
                IsolationLevel.ReadCommitted,
                cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (this.currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await this.SaveChangesAsync(cancellationToken);
                await this.currentTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await this.RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                this.currentTransaction.Dispose();
                this.currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (this.currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await this.currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                this.currentTransaction.Dispose();
                this.currentTransaction = null;
            }
        }

        public void ClearChangeTracker()
        {
            this.ChangeTracker.Clear();
        }

        public void DetachEntity(object entity)
        {
            this.Entry(entity).State = EntityState.Detached;
        }

        public (int Added, int Modified, int Deleted) GetChangeTrackerStatistics()
        {
            var entries = this.ChangeTracker.Entries().ToList();
            return (
                Added: entries.Count(e => e.State == EntityState.Added),
                Modified: entries.Count(e => e.State == EntityState.Modified),
                Deleted: entries.Count(e => e.State == EntityState.Deleted));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply all configurations from the current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Apply global configurations
            this.ApplyGlobalConfigurations(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Fallback configuration for design-time
                optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=EventManagementSystem;Trusted_Connection=true;");
            }

            // Enable detailed errors in development
            optionsBuilder.EnableDetailedErrors();
            optionsBuilder.EnableSensitiveDataLogging();

            // Configure connection resilience
            optionsBuilder.UseSqlServer(options =>
            {
                options.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

            base.OnConfiguring(optionsBuilder);
        }

        private void ApplyGlobalConfigurations(ModelBuilder modelBuilder)
        {
            // Configure precision for all decimal properties
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }

            // Configure string length for properties without explicit configuration
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(string) && p.GetMaxLength() == null))
            {
                property.SetMaxLength(450); // Default max length
            }

            // Configure cascading delete behavior
            foreach (var relationship in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        private void UpdateAuditFields()
        {
            var entries = this.ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.SetCreatedAt(DateTime.UtcNow);
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.MarkAsUpdated();
                }
            }
        }

        private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
        {
            if (this.domainEventDispatcher == null)
            {
                return;
            }

            var domainEntities = this.ChangeTracker.Entries<IAggregateRoot>()
                .Where(x => x.Entity.DomainEvents.Any())
                .Select(x => x.Entity)
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.DomainEvents)
                .ToList();

            // Clear domain events from entities
            foreach (var entity in domainEntities)
            {
                entity.ClearDomainEvents();
            }

            // Dispatch all domain events
            foreach (var domainEvent in domainEvents)
            {
                await this.domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);
            }
        }
    }
}
