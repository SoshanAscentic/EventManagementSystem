// <copyright file="IEventRegistrationRepository.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Repositories
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.ValueObjects;

    public interface IEventRegistrationRepository : IGenericRepository<EventRegistration>
    {
        // Registration-specific query operations
        Task<EventRegistration?> GetByIdAsync(RegistrationId id, CancellationToken cancellationToken = default);

        Task<EventRegistration?> GetByUserAndEventAsync(
            UserId userId,
            EventId eventId,
            CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(RegistrationId id, CancellationToken cancellationToken = default);

        Task<bool> IsUserRegisteredForEventAsync(
            UserId userId,
            EventId eventId,
            CancellationToken cancellationToken = default);

        // User-based queries
        Task<IReadOnlyList<EventRegistration>> GetByUserIdAsync(
            UserId userId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EventRegistration>> GetActiveRegistrationsByUserAsync(
            UserId userId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EventRegistration>> GetUserRegistrationHistoryAsync(
            UserId userId,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        // Event-based queries
        Task<IReadOnlyList<EventRegistration>> GetByEventIdAsync(
            EventId eventId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EventRegistration>> GetActiveRegistrationsByEventAsync(
            EventId eventId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EventRegistration>> GetCancelledRegistrationsByEventAsync(
            EventId eventId,
            CancellationToken cancellationToken = default);

        Task<int> GetActiveRegistrationCountForEventAsync(
            EventId eventId,
            CancellationToken cancellationToken = default);

        // Status-based queries
        Task<IReadOnlyList<EventRegistration>> GetRegistrationsByStatusAsync(
            RegistrationStatus status,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EventRegistration>> GetRegistrationsWithUpcomingEventsAsync(
            UserId? userId = null,
            CancellationToken cancellationToken = default);

        // Date-based queries
        Task<IReadOnlyList<EventRegistration>> GetRegistrationsByDateRangeAsync(
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EventRegistration>> GetRecentRegistrationsAsync(
            int count = 50,
            CancellationToken cancellationToken = default);

        // Analytics queries
        Task<Dictionary<EventId, int>> GetRegistrationCountsByEventAsync(
            IEnumerable<EventId> eventIds,
            CancellationToken cancellationToken = default);

        Task<Dictionary<UserId, int>> GetRegistrationCountsByUserAsync(
            IEnumerable<UserId> userIds,
            DateTime? fromDate = null,
            CancellationToken cancellationToken = default);

        Task<Dictionary<DateTime, int>> GetRegistrationTrendAsync(
            DateTime fromDate,
            DateTime toDate,
            CancellationToken cancellationToken = default);

        Task<Dictionary<RegistrationStatus, int>> GetRegistrationStatusStatsAsync(
            EventId? eventId = null,
            DateTime? fromDate = null,
            CancellationToken cancellationToken = default);

        // Attendance tracking
        Task<IReadOnlyList<EventRegistration>> GetAttendeesForEventAsync(
            EventId eventId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EventRegistration>> GetNoShowsForEventAsync(
            EventId eventId,
            CancellationToken cancellationToken = default);

        Task<double> GetAttendanceRateForEventAsync(
            EventId eventId,
            CancellationToken cancellationToken = default);

        // Bulk operations for performance
        Task<IReadOnlyList<EventRegistration>> GetByIdsAsync(
            IEnumerable<RegistrationId> registrationIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EventRegistration>> GetByUserIdsAsync(
            IEnumerable<UserId> userIds,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<EventRegistration>> GetByEventIdsAsync(
            IEnumerable<EventId> eventIds,
            CancellationToken cancellationToken = default);

        // Advanced filtering with multiple criteria
        Task<(IReadOnlyList<EventRegistration> Registrations, int TotalCount)> SearchRegistrationsAsync(
            UserId? userId = null,
            EventId? eventId = null,
            RegistrationStatus? status = null,
            DateTime? registeredAfter = null,
            DateTime? registeredBefore = null,
            int pageNumber = 1,
            int pageSize = 20,
            string sortBy = "RegisteredAt",
            bool ascending = false,
            CancellationToken cancellationToken = default);
    }
}
