// <copyright file="IUserRepository.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.ValueObjects;

    public interface IUserRepository : IGenericRepository<User>
    {
        // User-specific query operations
        Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);

        Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default);

        Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

        // User search and filtering
        Task<IReadOnlyList<User>> GetByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<User> Users, int TotalCount)> SearchUsersAsync(
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        // User activity queries
        Task<IReadOnlyList<User>> GetUsersWithActiveRegistrationsAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<User>> GetUsersRegisteredForEventAsync(EventId eventId, CancellationToken cancellationToken = default);

        Task<int> GetActiveUsersCountAsync(DateTime fromDate, CancellationToken cancellationToken = default);

        // User registration statistics
        Task<Dictionary<UserId, int>> GetUserRegistrationCountsAsync(
            IEnumerable<UserId> userIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<User>> GetTopActiveUsersAsync(
            int count = 10,
            DateTime? fromDate = null,
            CancellationToken cancellationToken = default);

        // Bulk operations for performance
        Task<IReadOnlyList<User>> GetByIdsAsync(
            IEnumerable<UserId> userIds,
            CancellationToken cancellationToken = default);

        Task<Dictionary<Email, User>> GetByEmailsAsync(
            IEnumerable<Email> emails,
            CancellationToken cancellationToken = default);
    }
}
