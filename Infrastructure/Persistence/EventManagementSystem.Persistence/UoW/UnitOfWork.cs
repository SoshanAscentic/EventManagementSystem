// <copyright file="UnitOfWork.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.UoW
{
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Persistence.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using Microsoft.Extensions.Logging;

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<UnitOfWork> logger;
        private IDbContextTransaction? transaction;
        private bool disposed;

        public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public bool HasActiveTransaction => this.transaction != null;

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var changes = this.context.ChangeTracker.Entries()
                    .Count(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

                this.logger.LogDebug("Saving {Changes} changes to database", changes);

                var result = await this.context.SaveChangesAsync(cancellationToken);

                this.logger.LogDebug("Successfully saved {Result} changes to database", result);
                return result;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                this.logger.LogError(ex, "Concurrency error occurred while saving changes");
                throw new InvalidOperationException("The data was modified by another process. Please refresh and try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                this.logger.LogError(ex, "Database error occurred while saving changes");
                throw new InvalidOperationException("A database error occurred while saving changes.", ex);
            }
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (this.transaction != null)
            {
                throw new InvalidOperationException("A transaction is already active.");
            }

            this.logger.LogDebug("Beginning database transaction");
            this.transaction = await this.context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (this.transaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }

            try
            {
                await this.SaveChangesAsync(cancellationToken);
                await this.transaction.CommitAsync(cancellationToken);
                this.logger.LogDebug("Database transaction committed successfully");
            }
            catch
            {
                await this.RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                await this.transaction.DisposeAsync();
                this.transaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (this.transaction == null)
            {
                this.logger.LogWarning("Attempted to rollback transaction but no active transaction exists");
                return;
            }

            try
            {
                await this.transaction.RollbackAsync(cancellationToken);
                this.logger.LogDebug("Database transaction rolled back");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error occurred while rolling back transaction");
            }
            finally
            {
                await this.transaction.DisposeAsync();
                this.transaction = null;
            }
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
        {
            var wasTransactionStarted = false;

            if (this.transaction == null)
            {
                await this.BeginTransactionAsync(cancellationToken);
                wasTransactionStarted = true;
            }

            try
            {
                var result = await operation();

                if (wasTransactionStarted)
                {
                    await this.CommitTransactionAsync(cancellationToken);
                }

                return result;
            }
            catch
            {
                if (wasTransactionStarted)
                {
                    await this.RollbackTransactionAsync(cancellationToken);
                }

                throw;
            }
        }

        public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
        {
            await this.ExecuteInTransactionAsync(
                async () =>
            {
                await operation();
                return 0;
            }, cancellationToken);
        }

        public void ClearChangeTracker()
        {
            this.context.ChangeTracker.Clear();
            this.logger.LogDebug("Change tracker cleared");
        }

        public void DetachEntity(object entity)
        {
            var entry = this.context.Entry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Detached;
                this.logger.LogDebug("Entity detached: {EntityType}", entity.GetType().Name);
            }
        }

        public (int Added, int Modified, int Deleted) GetChangeTrackerStatistics()
        {
            var entries = this.context.ChangeTracker.Entries().ToList();

            return (
                Added: entries.Count(e => e.State == EntityState.Added),
                Modified: entries.Count(e => e.State == EntityState.Modified),
                Deleted: entries.Count(e => e.State == EntityState.Deleted));
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                this.transaction?.Dispose();
                this.context.Dispose();
                this.disposed = true;
            }
        }
    }
}
