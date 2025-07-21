// <copyright file="EventCategoryRepository.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Repositories
{
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Persistence.Context;
    using Microsoft.EntityFrameworkCore;

    public class EventCategoryRepository : BaseRepository<EventCategory>, IEventCategoryRepository
    {
        public EventCategoryRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<EventCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<EventCategory?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
        }

        public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.AnyAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.AnyAsync(c => c.Name == name, cancellationToken);
        }

        public async Task<IReadOnlyList<EventCategory>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventCategory>> GetInactiveAsync(CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Where(c => !c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventCategory>> GetWithEventCountsAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
        {
            var query = this.dbSet.Include(c => c.Events).AsQueryable();

            if (activeOnly)
            {
                query = query.Where(c => c.IsActive);
            }

            return await query
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<EventCategory?> GetByIdWithEventsAsync(int id, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Dictionary<int, int>> GetCategoryEventCountsAsync(IEnumerable<int> categoryIds, DateTime? fromDate = null, CancellationToken cancellationToken = default)
        {
            var query = this.context.Events.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(e => e.CreatedAt >= fromDate.Value);
            }

            return await query
                .Where(e => categoryIds.Contains(e.CategoryId))
                .GroupBy(e => e.CategoryId)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
        }

        public async Task<IReadOnlyList<EventCategory>> GetPopularCategoriesAsync(int count = 10, DateTime? fromDate = null, CancellationToken cancellationToken = default)
        {
            var query = this.dbSet
                .Include(c => c.Events)
                .Where(c => c.IsActive);

            if (fromDate.HasValue)
            {
                query = query.Where(c => c.Events.Any(e => e.CreatedAt >= fromDate.Value));
            }

            return await query
                .OrderByDescending(c => c.Events.Count)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventCategory>> GetCategoriesWithUpcomingEventsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await this.dbSet
                .Include(c => c.Events.Where(e => e.EventDateTime.StartDateTime > now))
                .Where(c => c.IsActive && c.Events.Any(e => e.EventDateTime.StartDateTime > now))
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<EventCategory>> GetByIdsAsync(IEnumerable<int> categoryIds, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Where(c => categoryIds.Contains(c.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<string, EventCategory>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Where(c => names.Contains(c.Name))
                .ToDictionaryAsync(c => c.Name, c => c, cancellationToken);
        }
    }
}
