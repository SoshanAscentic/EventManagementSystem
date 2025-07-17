// <copyright file="IEventRepository.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Repositories
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.Enums;
    using EventManagementSystem.Domain.ValueObjects;

    public interface IEventRepository : IGenericRepository<Event>
    {
        // Event-specific query operations
        Task<Event?> GetByIdAsync(EventId id, CancellationToken cancellationToken = default);

        Task<Event?> GetByIdWithRegistrationsAsync(EventId id, CancellationToken cancellationToken = default);

        Task<Event?> GetByIdWithImagesAsync(EventId id, CancellationToken cancellationToken = default);

        Task<Event?> GetByIdWithAllDetailsAsync(EventId id, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(EventId id, CancellationToken cancellationToken = default);

        Task<bool> ExistsByTitleAndDateAsync(string title, DateTime startDate, CancellationToken cancellationToken = default);

        // Event filtering and search
        Task<IReadOnlyList<Event>> GetUpcomingEventsAsync(
            int? categoryId = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Event>> GetEventsByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Event>> GetEventsByCategoryAsync(
            int categoryId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Event>> GetEventsByTypeAsync(
            EventType eventType,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Event>> GetEventsByLocationAsync(
            string searchTerm,
            CancellationToken cancellationToken = default);

        // Advanced search with multiple filters
        Task<(IReadOnlyList<Event> Events, int TotalCount)> SearchEventsAsync(
            string? searchTerm = null,
            int? categoryId = null,
            EventType? eventType = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? location = null,
            bool? hasAvailableSpots = null,
            int pageNumber = 1,
            int pageSize = 20,
            string sortBy = "StartDateTime",
            bool ascending = true,
            CancellationToken cancellationToken = default);

        // Event status queries
        Task<IReadOnlyList<Event>> GetOngoingEventsAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Event>> GetCompletedEventsAsync(
            DateTime? fromDate = null,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Event>> GetEventsWithOpenRegistrationAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Event>> GetFullEventsAsync(CancellationToken cancellationToken = default);

        // Event registration queries
        Task<IReadOnlyList<Event>> GetEventsWithRegistrationDeadlineAsync(
            TimeSpan timeWindow,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Event>> GetEventsStartingSoonAsync(
            TimeSpan timeWindow,
            CancellationToken cancellationToken = default);

        // Analytics queries
        Task<Dictionary<int, int>> GetEventRegistrationCountsAsync(
            IEnumerable<EventId> eventIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Event>> GetPopularEventsAsync(
            int count = 10,
            DateTime? fromDate = null,
            CancellationToken cancellationToken = default);

        Task<Dictionary<string, int>> GetEventStatsByLocationAsync(
            DateTime? fromDate = null,
            CancellationToken cancellationToken = default);

        Task<Dictionary<EventType, int>> GetEventStatsByTypeAsync(
            DateTime? fromDate = null,
            CancellationToken cancellationToken = default);

        // Bulk operations for performance
        Task<IReadOnlyList<Event>> GetByIdsAsync(
            IEnumerable<EventId> eventIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Event>> GetByIdsWithRegistrationsAsync(
            IEnumerable<EventId> eventIds,
            CancellationToken cancellationToken = default);
    }
}
