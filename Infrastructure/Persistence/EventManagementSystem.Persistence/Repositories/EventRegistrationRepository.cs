// <copyright file="EventRegistrationRepository.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Repositories
{
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using EventManagementSystem.Persistence.Context;
    using Microsoft.EntityFrameworkCore;

    public class EventRegistrationRepository : BaseRepository<EventRegistration>, IEventRegistrationRepository
    {
        public EventRegistrationRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<EventRegistration?> GetByIdAsync(RegistrationId id, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id.Value, cancellationToken);
        }

        public async Task<EventRegistration?> GetByUserAndEventAsync(UserId userId, EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.UserId.Value == userId.Value && r.EventId.Value == eventId.Value, cancellationToken);
        }

        public async Task<bool> ExistsAsync(RegistrationId id, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.AnyAsync(r => r.Id == id.Value, cancellationToken);
        }

        public async Task<bool> IsUserRegisteredForEventAsync(UserId userId, EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.AnyAsync(
                r =>
                r.UserId.Value == userId.Value &&
                r.EventId.Value == eventId.Value &&
                r.Status.Value == "Registered",
                cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.UserId.Value == userId.Value)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetActiveRegistrationsByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.UserId.Value == userId.Value && r.Status.Value == "Registered")
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetUserRegistrationHistoryAsync(UserId userId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                    .ThenInclude(e => e.Category)
                .Include(r => r.User)
                .Where(r => r.UserId.Value == userId.Value)
                .OrderByDescending(r => r.RegisteredAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetByEventIdAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.EventId.Value == eventId.Value)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetActiveRegistrationsByEventAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.EventId.Value == eventId.Value && r.Status.Value == "Registered")
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetCancelledRegistrationsByEventAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.EventId.Value == eventId.Value && r.Status.Value == "Cancelled")
                .OrderByDescending(r => r.CancelledAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetActiveRegistrationCountForEventAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.CountAsync(
                r =>
                r.EventId.Value == eventId.Value &&
                r.Status.Value == "Registered",
                cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetRegistrationsByStatusAsync(RegistrationStatus status, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.Status.Value == status.Value)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetRegistrationsWithUpcomingEventsAsync(UserId? userId = null, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var query = this.dbSet
                .Include(r => r.Event)
                    .ThenInclude(e => e.Category)
                .Include(r => r.User)
                .Where(r => r.Status.Value == "Registered" && r.Event!.EventDateTime.StartDateTime > now);

            if (userId != null)
            {
                query = query.Where(r => r.UserId.Value == userId.Value);
            }

            return await query
                .OrderBy(r => r.Event!.EventDateTime.StartDateTime)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetRegistrationsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.RegisteredAt >= startDate && r.RegisteredAt <= endDate)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetRecentRegistrationsAsync(int count = 50, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .OrderByDescending(r => r.RegisteredAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<EventId, int>> GetRegistrationCountsByEventAsync(IEnumerable<EventId> eventIds, CancellationToken cancellationToken = default)
        {
            var ids = eventIds.Select(e => e.Value).ToList();
            return await this.dbSet
                .Where(r => ids.Contains(r.EventId.Value) && r.Status.Value == "Registered")
                .GroupBy(r => r.EventId.Value)
                .ToDictionaryAsync(g => EventId.Create(g.Key), g => g.Count(), cancellationToken);
        }

        public async Task<Dictionary<UserId, int>> GetRegistrationCountsByUserAsync(IEnumerable<UserId> userIds, DateTime? fromDate = null, CancellationToken cancellationToken = default)
        {
            var ids = userIds.Select(u => u.Value).ToList();
            var query = this.dbSet.Where(r => ids.Contains(r.UserId.Value));

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.RegisteredAt >= fromDate.Value);
            }

            return await query
                .GroupBy(r => r.UserId.Value)
                .ToDictionaryAsync(g => UserId.Create(g.Key), g => g.Count(), cancellationToken);
        }

        public async Task<Dictionary<DateTime, int>> GetRegistrationTrendAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Where(r => r.RegisteredAt >= fromDate && r.RegisteredAt <= toDate)
                .GroupBy(r => r.RegisteredAt.Date)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
        }

        public async Task<Dictionary<RegistrationStatus, int>> GetRegistrationStatusStatsAsync(EventId? eventId = null, DateTime? fromDate = null, CancellationToken cancellationToken = default)
        {
            var query = this.dbSet.AsQueryable();

            if (eventId != null)
            {
                query = query.Where(r => r.EventId.Value == eventId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.RegisteredAt >= fromDate.Value);
            }

            var stats = await query
                .GroupBy(r => r.Status.Value)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);

            return stats.ToDictionary(
                kvp => RegistrationStatus.Create(kvp.Key),
                kvp => kvp.Value);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetAttendeesForEventAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.EventId.Value == eventId.Value && r.Status.Value == "Attended")
                .OrderBy(r => r.User!.LastName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetNoShowsForEventAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => r.EventId.Value == eventId.Value && r.Status.Value == "NoShow")
                .OrderBy(r => r.User!.LastName)
                .ToListAsync(cancellationToken);
        }

        public async Task<double> GetAttendanceRateForEventAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            var totalRegistrations = await this.dbSet.CountAsync(r =>
                r.EventId.Value == eventId.Value &&
                (r.Status.Value == "Attended" || r.Status.Value == "NoShow"),
                cancellationToken);

            if (totalRegistrations == 0)
                return 0;

            var attendees = await this.dbSet.CountAsync(r =>
                r.EventId.Value == eventId.Value &&
                r.Status.Value == "Attended",
                cancellationToken);

            return (double)attendees / totalRegistrations * 100;
        }

        public async Task<IReadOnlyList<EventRegistration>> GetByIdsAsync(IEnumerable<RegistrationId> registrationIds, CancellationToken cancellationToken = default)
        {
            var ids = registrationIds.Select(r => r.Value).ToList();
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => ids.Contains(r.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetByUserIdsAsync(IEnumerable<UserId> userIds, CancellationToken cancellationToken = default)
        {
            var ids = userIds.Select(u => u.Value).ToList();
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => ids.Contains(r.UserId.Value))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetByEventIdsAsync(IEnumerable<EventId> eventIds, CancellationToken cancellationToken = default)
        {
            var ids = eventIds.Select(e => e.Value).ToList();
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => ids.Contains(r.EventId.Value))
                .ToListAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<EventRegistration> Registrations, int TotalCount)> SearchRegistrationsAsync(
            UserId? userId = null,
            EventId? eventId = null,
            RegistrationStatus? status = null,
            DateTime? registeredAfter = null,
            DateTime? registeredBefore = null,
            int pageNumber = 1,
            int pageSize = 20,
            string sortBy = "RegisteredAt",
            bool ascending = false,
            CancellationToken cancellationToken = default)
        {
            var query = this.dbSet
                .Include(r => r.Event)
                    .ThenInclude(e => e.Category)
                .Include(r => r.User)
                .AsQueryable();

            // Apply filters
            if (userId != null)
            {
                query = query.Where(r => r.UserId.Value == userId.Value);
            }

            if (eventId != null)
            {
                query = query.Where(r => r.EventId.Value == eventId.Value);
            }

            if (status != null)
            {
                query = query.Where(r => r.Status.Value == status.Value);
            }

            if (registeredAfter.HasValue)
            {
                query = query.Where(r => r.RegisteredAt >= registeredAfter.Value);
            }

            if (registeredBefore.HasValue)
            {
                query = query.Where(r => r.RegisteredAt <= registeredBefore.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = sortBy.ToLowerInvariant() switch
            {
                "username" => ascending ? query.OrderBy(r => r.User!.LastName) : query.OrderByDescending(r => r.User!.LastName),
                "eventtitle" => ascending ? query.OrderBy(r => r.Event!.Title) : query.OrderByDescending(r => r.Event!.Title),
                "status" => ascending ? query.OrderBy(r => r.Status.Value) : query.OrderByDescending(r => r.Status.Value),
                "cancelledat" => ascending ? query.OrderBy(r => r.CancelledAt) : query.OrderByDescending(r => r.CancelledAt),
                _ => ascending ? query.OrderBy(r => r.RegisteredAt) : query.OrderByDescending(r => r.RegisteredAt)
            };

            var registrations = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (registrations, totalCount);
        }
    }
}