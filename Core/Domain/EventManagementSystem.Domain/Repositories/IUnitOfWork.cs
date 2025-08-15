// <copyright file="IUnitOfWork.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        bool HasActiveTransaction { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        Task<T> ExecuteInTransactionAsync<T>(
            Func<Task<T>> operation,
            CancellationToken cancellationToken = default);

        Task ExecuteInTransactionAsync(
            Func<Task> operation,
            CancellationToken cancellationToken = default);

        void ClearChangeTracker();

        void DetachEntity(object entity);

        (int Added, int Modified, int Deleted) GetChangeTrackerStatistics();
    }
}
