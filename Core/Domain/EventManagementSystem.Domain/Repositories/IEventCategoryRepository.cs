// <copyright file="IEventCategoryRepository.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Repositories
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.Entities;

    public interface IEventCategoryRepository : IGenericRepository<EventCategory>
    {
        // Category-specific query operations
        Task<EventCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<EventCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

        // Active/Inactive categories
        Task<IReadOnlyList<EventCategory>> GetActiveAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EventCategory>> GetInactiveAsync(CancellationToken cancellationToken = default);

        // Categories with event counts
        Task<IReadOnlyList<EventCategory>> GetWithEventCountsAsync(
            bool activeOnly = true,
            CancellationToken cancellationToken = default);

        Task<EventCategory?> GetByIdWithEventsAsync(int id, CancellationToken cancellationToken = default);

        // Category usage analytics
        Task<Dictionary<int, int>> GetCategoryEventCountsAsync(
            IEnumerable<int> categoryIds,
            DateTime? fromDate = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EventCategory>> GetPopularCategoriesAsync(
            int count = 10,
            DateTime? fromDate = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EventCategory>> GetCategoriesWithUpcomingEventsAsync(
            CancellationToken cancellationToken = default);

        // Bulk operations
        Task<IReadOnlyList<EventCategory>> GetByIdsAsync(
            IEnumerable<int> categoryIds,
            CancellationToken cancellationToken = default);

        Task<Dictionary<string, EventCategory>> GetByNamesAsync(
            IEnumerable<string> names,
            CancellationToken cancellationToken = default);
    }
}
