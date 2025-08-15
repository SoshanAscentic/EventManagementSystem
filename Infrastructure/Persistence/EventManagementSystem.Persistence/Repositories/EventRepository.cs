// <copyright file="EventRepository.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Repositories
{
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using EventManagementSystem.Persistence.Context;
    using Microsoft.EntityFrameworkCore;

    public class EventRepository : BaseRepository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<Event?> GetByIdAsync(EventId id, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id.Value, cancellationToken);
        }

        public async Task<Event?> GetByIdWithRegistrationsAsync(EventId id, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Registrations.Where(r => EF.Property<string>(r, "_status") == "Registered"))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(e => e.Id == id.Value, cancellationToken);
        }

        public async Task<Event?> GetByIdWithImagesAsync(EventId id, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Images)
                .FirstOrDefaultAsync(e => e.Id == id.Value, cancellationToken);
        }

        public async Task<Event?> GetByIdWithAllDetailsAsync(EventId id, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Registrations.Where(r => EF.Property<string>(r, "_status") == "Registered"))
                    .ThenInclude(r => r.User)
                .Include(e => e.Images)
                .FirstOrDefaultAsync(e => e.Id == id.Value, cancellationToken);
        }

        public async Task<bool> ExistsAsync(EventId id, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.AnyAsync(e => e.Id == id.Value, cancellationToken);
        }

        public async Task<bool> ExistsByTitleAndDateAsync(string title, DateTime startDate, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.AnyAsync(
                e =>
                e.Title == title &&
                EF.Property<DateTime>(e, "_startDateTime").Date == startDate.Date,
                cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetUpcomingEventsAsync(int? categoryId = null, CancellationToken cancellationToken = default)
        {
            var query = this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Images.Where(i => i.IsPrimary))
                .Where(e => EF.Property<DateTime>(e, "_startDateTime") > DateTime.UtcNow);

            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            return await query
                .OrderBy(e => EF.Property<DateTime>(e, "_startDateTime"))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetEventsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Images.Where(i => i.IsPrimary))
                .Where(e => EF.Property<DateTime>(e, "_startDateTime") >= startDate &&
                           EF.Property<DateTime>(e, "_startDateTime") <= endDate)
                .OrderBy(e => EF.Property<DateTime>(e, "_startDateTime"))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetEventsByCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Images.Where(i => i.IsPrimary))
                .Where(e => e.CategoryId == categoryId)
                .OrderBy(e => EF.Property<DateTime>(e, "_startDateTime"))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetEventsByTypeAsync(EventType eventType, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Images.Where(i => i.IsPrimary))
                .Where(e => EF.Property<string>(e, "_eventType") == eventType.Value)
                .OrderBy(e => EF.Property<DateTime>(e, "_startDateTime"))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetEventsByLocationAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Images.Where(i => i.IsPrimary))
                .Where(e =>
                    EF.Property<string>(e, "_venue").Contains(searchTerm) ||
                    EF.Property<string>(e, "_address").Contains(searchTerm) ||
                    (EF.Property<string>(e, "_city") != null && EF.Property<string>(e, "_city").Contains(searchTerm)) ||
                    (EF.Property<string>(e, "_country") != null && EF.Property<string>(e, "_country").Contains(searchTerm)))
                .OrderBy(e => EF.Property<DateTime>(e, "_startDateTime"))
                .ToListAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<Event> Events, int TotalCount)> SearchEventsAsync(
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
            CancellationToken cancellationToken = default)
        {
            var query = this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Images.Where(i => i.IsPrimary))
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e =>
                    e.Title.Contains(searchTerm) ||
                    e.Description.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            if (eventType != null)
            {
                query = query.Where(e => EF.Property<string>(e, "_eventType") == eventType.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => EF.Property<DateTime>(e, "_startDateTime") >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => EF.Property<DateTime>(e, "_startDateTime") <= endDate.Value);
            }

            if (!string.IsNullOrEmpty(location))
            {
                query = query.Where(e =>
                    EF.Property<string>(e, "_venue").Contains(location) ||
                    EF.Property<string>(e, "_address").Contains(location) ||
                    (EF.Property<string>(e, "_city") != null && EF.Property<string>(e, "_city").Contains(location)) ||
                    (EF.Property<string>(e, "_country") != null && EF.Property<string>(e, "_country").Contains(location)));
            }

            if (hasAvailableSpots.HasValue && hasAvailableSpots.Value)
            {
                // This would require a subquery or computed column
                query = query.Where(e =>
                    e.Registrations.Count(r => EF.Property<string>(r, "_status") == "Registered") < EF.Property<int>(e, "_capacity"));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = sortBy.ToLowerInvariant() switch
            {
                "title" => ascending ? query.OrderBy(e => e.Title) : query.OrderByDescending(e => e.Title),
                "category" => ascending ? query.OrderBy(e => e.Category!.Name) : query.OrderByDescending(e => e.Category!.Name),
                "capacity" => ascending ? query.OrderBy(e => EF.Property<int>(e, "_capacity")) : query.OrderByDescending(e => EF.Property<int>(e, "_capacity")),
                "createdat" => ascending ? query.OrderBy(e => e.CreatedAt) : query.OrderByDescending(e => e.CreatedAt),
                _ => ascending ? query.OrderBy(e => EF.Property<DateTime>(e, "_startDateTime")) : query.OrderByDescending(e => EF.Property<DateTime>(e, "_startDateTime"))
            };

            var events = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (events, totalCount);
        }

        public async Task<IReadOnlyList<Event>> GetOngoingEventsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await this.dbSet
                .Include(e => e.Category)
                .Where(e => EF.Property<DateTime>(e, "_startDateTime") <= now &&
                           EF.Property<DateTime>(e, "_endDateTime") >= now)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetCompletedEventsAsync(DateTime? fromDate = null, CancellationToken cancellationToken = default)
        {
            var query = this.dbSet
                .Include(e => e.Category)
                .Where(e => EF.Property<DateTime>(e, "_endDateTime") < DateTime.UtcNow);

            if (fromDate.HasValue)
            {
                query = query.Where(e => EF.Property<DateTime>(e, "_endDateTime") >= fromDate.Value);
            }

            return await query
                .OrderByDescending(e => EF.Property<DateTime>(e, "_endDateTime"))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetEventsWithOpenRegistrationAsync(CancellationToken cancellationToken = default)
        {
            var registrationCutoff = DateTime.UtcNow.AddHours(2); // 2 hours before event
            return await this.dbSet
                .Include(e => e.Category)
                .Where(e => EF.Property<DateTime>(e, "_startDateTime") > registrationCutoff)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetFullEventsAsync(CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Registrations.Where(r => EF.Property<string>(r, "_status") == "Registered"))
                .Where(e => e.Registrations.Count(r => EF.Property<string>(r, "_status") == "Registered") >= EF.Property<int>(e, "_capacity"))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetEventsWithRegistrationDeadlineAsync(TimeSpan timeWindow, CancellationToken cancellationToken = default)
        {
            var cutoffTime = DateTime.UtcNow.Add(timeWindow);
            return await this.dbSet
                .Include(e => e.Category)
                .Where(e => EF.Property<DateTime>(e, "_startDateTime").AddHours(-2) <= cutoffTime &&
                           EF.Property<DateTime>(e, "_startDateTime") > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetEventsStartingSoonAsync(TimeSpan timeWindow, CancellationToken cancellationToken = default)
        {
            var startTime = DateTime.UtcNow.Add(timeWindow);
            return await this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Registrations.Where(r => EF.Property<string>(r, "_status") == "Registered"))
                    .ThenInclude(r => r.User)
                .Where(e => EF.Property<DateTime>(e, "_startDateTime") <= startTime &&
                           EF.Property<DateTime>(e, "_startDateTime") > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<int, int>> GetEventRegistrationCountsAsync(IEnumerable<EventId> eventIds, CancellationToken cancellationToken = default)
        {
            var ids = eventIds.Select(e => e.Value).ToList();
            return await this.context.EventRegistrations
                .Where(r => ids.Contains(EF.Property<int>(r, "_eventId")) && EF.Property<string>(r, "_status") == "Registered")
                .GroupBy(r => EF.Property<int>(r, "_eventId"))
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetPopularEventsAsync(int count = 10, DateTime? fromDate = null, CancellationToken cancellationToken = default)
        {
            var query = this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Images.Where(i => i.IsPrimary))
                .Include(e => e.Registrations.Where(r => EF.Property<string>(r, "_status") == "Registered"));

            if (fromDate.HasValue)
            {
                query = (Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<Event, IEnumerable<EventRegistration>>)query.Where(e => e.CreatedAt >= fromDate.Value);
            }

            return await query
                .OrderByDescending(e => e.Registrations.Count(r => EF.Property<string>(r, "_status") == "Registered"))
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<string, int>> GetEventStatsByLocationAsync(DateTime? fromDate = null, CancellationToken cancellationToken = default)
        {
            var query = this.dbSet.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.CreatedAt >= fromDate.Value);
            }

            return await query
                .GroupBy(e => EF.Property<string>(e, "_city") ?? "Unknown")
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
        }

        public async Task<Dictionary<EventType, int>> GetEventStatsByTypeAsync(DateTime? fromDate = null, CancellationToken cancellationToken = default)
        {
            var query = this.dbSet.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.CreatedAt >= fromDate.Value);
            }

            var stats = await query
                .GroupBy(e => EF.Property<string>(e, "_eventType"))
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);

            return stats.ToDictionary(
                kvp => EventType.Create(kvp.Key),
                kvp => kvp.Value);
        }

        public async Task<IReadOnlyList<Event>> GetByIdsAsync(IEnumerable<EventId> eventIds, CancellationToken cancellationToken = default)
        {
            var ids = eventIds.Select(e => e.Value).ToList();
            return await this.dbSet
                .Include(e => e.Category)
                .Where(e => ids.Contains(e.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Event>> GetByIdsWithRegistrationsAsync(IEnumerable<EventId> eventIds, CancellationToken cancellationToken = default)
        {
            var ids = eventIds.Select(e => e.Value).ToList();
            return await this.dbSet
                .Include(e => e.Category)
                .Include(e => e.Registrations.Where(r => EF.Property<string>(r, "_status") == "Registered"))
                    .ThenInclude(r => r.User)
                .Where(e => ids.Contains(e.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
