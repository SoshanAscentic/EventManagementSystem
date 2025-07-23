// <copyright file="UserRepository.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Repositories
{
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using EventManagementSystem.Persistence.Context;
    using Microsoft.EntityFrameworkCore;

    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default)
        {
            // Fixed: Use the backing field instead of the value object property
            return await this.dbSet
                .Include(u => u.Registrations.Where(r => EF.Property<string>(r, "_status") == "Registered"))
                .FirstOrDefaultAsync(u => u.Id == id.Value, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            // Fixed: Use the backing field for email comparison
            return await this.dbSet
                .Include(u => u.Registrations.Where(r => EF.Property<string>(r, "_status") == "Registered"))
                .FirstOrDefaultAsync(u => EF.Property<string>(u, "_email") == email.Value, cancellationToken);
        }

        public async Task<bool> ExistsAsync(UserId id, CancellationToken cancellationToken = default)
        {
            return await this.dbSet.AnyAsync(u => u.Id == id.Value, cancellationToken);
        }

        public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        {
            // Fixed: Use the backing field for email comparison
            return await this.dbSet.AnyAsync(u => EF.Property<string>(u, "_email") == email.Value, cancellationToken);
        }

        public async Task<IReadOnlyList<User>> GetByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Where(u =>
                    u.FirstName.Contains(searchTerm) ||
                    u.LastName.Contains(searchTerm) ||
                    (u.FirstName + " " + u.LastName).Contains(searchTerm))
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IReadOnlyList<User> Users, int TotalCount)> SearchUsersAsync(
            string? searchTerm = null,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = this.dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Fixed: Use backing field for email search
                query = query.Where(u =>
                    u.FirstName.Contains(searchTerm) ||
                    u.LastName.Contains(searchTerm) ||
                    EF.Property<string>(u, "_email").Contains(searchTerm) ||
                    (u.FirstName + " " + u.LastName).Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderBy(u => u.LastName)
                .ThenBy(u => u.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (users, totalCount);
        }

        public async Task<IReadOnlyList<User>> GetUsersWithActiveRegistrationsAsync(CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(u => u.Registrations.Where(r => EF.Property<string>(r, "_status") == "Registered"))
                    .ThenInclude(r => r.Event)
                .Where(u => u.Registrations.Any(r => EF.Property<string>(r, "_status") == "Registered"))
                .OrderBy(u => u.LastName)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<User>> GetUsersRegisteredForEventAsync(EventId eventId, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Include(u => u.Registrations.Where(r => EF.Property<int>(r, "_eventId") == eventId.Value && EF.Property<string>(r, "_status") == "Registered"))
                .Where(u => u.Registrations.Any(r => EF.Property<int>(r, "_eventId") == eventId.Value && EF.Property<string>(r, "_status") == "Registered"))
                .OrderBy(u => u.LastName)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> GetActiveUsersCountAsync(DateTime fromDate, CancellationToken cancellationToken = default)
        {
            return await this.dbSet
                .Where(u => u.Registrations.Any(r => r.RegisteredAt >= fromDate))
                .CountAsync(cancellationToken);
        }

        public async Task<Dictionary<UserId, int>> GetUserRegistrationCountsAsync(IEnumerable<UserId> userIds, CancellationToken cancellationToken = default)
        {
            var ids = userIds.Select(u => u.Value).ToList();
            return await this.context.EventRegistrations
                .Where(r => ids.Contains(EF.Property<int>(r, "_userId")))
                .GroupBy(r => EF.Property<int>(r, "_userId"))
                .ToDictionaryAsync(g => UserId.Create(g.Key), g => g.Count(), cancellationToken);
        }

        public async Task<IReadOnlyList<User>> GetTopActiveUsersAsync(int count = 10, DateTime? fromDate = null, CancellationToken cancellationToken = default)
        {
            var query = this.dbSet
                .Include(u => u.Registrations)
                .AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(u => u.Registrations.Any(r => r.RegisteredAt >= fromDate.Value));
            }

            return await query
                .OrderByDescending(u => u.Registrations.Count)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<User>> GetByIdsAsync(IEnumerable<UserId> userIds, CancellationToken cancellationToken = default)
        {
            var ids = userIds.Select(u => u.Value).ToList();
            return await this.dbSet
                .Where(u => ids.Contains(u.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<Email, User>> GetByEmailsAsync(IEnumerable<Email> emails, CancellationToken cancellationToken = default)
        {
            var emailValues = emails.Select(e => e.Value).ToList();
            return await this.dbSet
                .Where(u => emailValues.Contains(EF.Property<string>(u, "_email")))
                .ToDictionaryAsync(u => u.Email, u => u, cancellationToken);
        }
    }
}
