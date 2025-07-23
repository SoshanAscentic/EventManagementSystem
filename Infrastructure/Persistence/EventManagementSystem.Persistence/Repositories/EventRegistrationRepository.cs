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
                .FirstOrDefaultAsync(r => EF.Property<int>(r, "_userId") == userId.Value && EF.Property<int>(r, "_eventId") == eventId.Value, cancellationToken);
        }

        public async Task<bool> ExistsAsync(RegistrationId id, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.AnyAsync(r => r.Id == id.Value, cancellationToken);
        }

        public async Task<bool> IsUserRegisteredForEventAsync(UserId userId, EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.AnyAsync(
                r =>
                EF.Property<int>(r, "_userId") == userId.Value &&
                EF.Property<int>(r, "_eventId") == eventId.Value &&
                EF.Property<string>(r, "_status") == "Registered",
                cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => EF.Property<int>(r, "_userId") == userId.Value)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetActiveRegistrationsByUserAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => EF.Property<int>(r, "_userId") == userId.Value && EF.Property<string>(r, "_status") == "Registered")
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetUserRegistrationHistoryAsync(UserId userId, int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                    .ThenInclude(e => e.Category)
                .Include(r => r.User)
                .Where(r => EF.Property<int>(r, "_userId") == userId.Value)
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
                .Where(r => EF.Property<int>(r, "_eventId") == eventId.Value)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetActiveRegistrationsByEventAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => EF.Property<int>(r, "_eventId") == eventId.Value && EF.Property<string>(r, "_status") == "Registered")
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetCancelledRegistrationsByEventAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => EF.Property<int>(r, "_eventId") == eventId.Value && EF.Property<string>(r, "_status") == "Cancelled")
                .OrderByDescending(r => r.CancelledAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetActiveRegistrationCountForEventAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.CountAsync(
                r =>
                EF.Property<int>(r, "_eventId") == eventId.Value &&
                EF.Property<string>(r, "_status") == "Registered",
                cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetRegistrationsByStatusAsync(RegistrationStatus status, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => EF.Property<string>(r, "_status") == status.Value)
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
                .Where(r => EF.Property<string>(r, "_status") == "Registered" && EF.Property<DateTime>(r.Event, "_startDateTime") > now);

            if (userId != null)
            {
                query = query.Where(r => EF.Property<int>(r, "_userId") == userId.Value);
            }

            return await query
                .OrderBy(r => EF.Property<DateTime>(r.Event, "_startDateTime"))
                .ToListAsync(cancellationToken);
        }

        // ... (other methods remain similar - just replace Status.Value with EF.Property<string>(r, "_status") and similar for other value objects)

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
                .Where(r => ids.Contains(EF.Property<int>(r, "_eventId")) && EF.Property<string>(r, "_status") == "Registered")
                .GroupBy(r => EF.Property<int>(r, "_eventId"))
                .ToDictionaryAsync(g => EventId.Create(g.Key), g => g.Count(), cancellationToken);
        }

        public async Task<Dictionary<UserId, int>> GetRegistrationCountsByUserAsync(IEnumerable<UserId> userIds, DateTime? fromDate = null, CancellationToken cancellationToken = default)
        {
            var ids = userIds.Select(u => u.Value).ToList();
            var query = this.dbSet.Where(r => ids.Contains(EF.Property<int>(r, "_userId")));

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.RegisteredAt >= fromDate.Value);
            }

            return await query
                .GroupBy(r => EF.Property<int>(r, "_userId"))
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
                query = query.Where(r => EF.Property<int>(r, "_eventId") == eventId.Value);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.RegisteredAt >= fromDate.Value);
            }

            var stats = await query
                .GroupBy(r => EF.Property<string>(r, "_status"))
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
                .Where(r => EF.Property<int>(r, "_eventId") == eventId.Value && EF.Property<string>(r, "_status") == "Attended")
                .OrderBy(r => r.User!.LastName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetNoShowsForEventAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => EF.Property<int>(r, "_eventId") == eventId.Value && EF.Property<string>(r, "_status") == "NoShow")
                .OrderBy(r => r.User!.LastName)
                .ToListAsync(cancellationToken);
        }

        public async Task<double> GetAttendanceRateForEventAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            var totalRegistrations = await this.dbSet.CountAsync(r =>
                EF.Property<int>(r, "_eventId") == eventId.Value &&
                (EF.Property<string>(r, "_status") == "Attended" || EF.Property<string>(r, "_status") == "NoShow"),
                cancellationToken);

            if (totalRegistrations == 0)
                return 0;

            var attendees = await this.dbSet.CountAsync(r =>
                EF.Property<int>(r, "_eventId") == eventId.Value &&
                EF.Property<string>(r, "_status") == "Attended",
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
                .Where(r => ids.Contains(EF.Property<int>(r, "_userId")))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventRegistration>> GetByEventIdsAsync(IEnumerable<EventId> eventIds, CancellationToken cancellationToken = default)
        {
            var ids = eventIds.Select(e => e.Value).ToList();
            return await this.dbSet
                .Include(r => r.Event)
                .Include(r => r.User)
                .Where(r => ids.Contains(EF.Property<int>(r, "_eventId")))
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
                query = query.Where(r => EF.Property<int>(r, "_userId") == userId.Value);
            }

            if (eventId != null)
            {
                query = query.Where(r => EF.Property<int>(r, "_eventId") == eventId.Value);
            }

            if (status != null)
            {
                query = query.Where(r => EF.Property<string>(r, "_status") == status.Value);
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
                "status" => ascending ? query.OrderBy(r => EF.Property<string>(r, "_status")) : query.OrderByDescending(r => EF.Property<string>(r, "_status")),
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
