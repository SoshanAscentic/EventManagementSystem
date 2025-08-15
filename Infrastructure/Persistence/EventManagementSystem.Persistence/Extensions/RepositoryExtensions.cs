// <copyright file="RepositoryExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Extensions
{
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Persistence.Context;
    using Microsoft.EntityFrameworkCore;

    public static class RepositoryExtensions
    {
        public static async Task<EventStatisticsDto> GetEventStatisticsAsync(
            this ApplicationDbContext context,
            DateTime? fromDate = null,
            CancellationToken cancellationToken = default)
        {
            fromDate ??= DateTime.UtcNow.AddMonths(-12);

            // Use backing fields in queries - FIXED
            var events = await context.Events
                .Where(e => e.CreatedAt >= fromDate)
                .Select(e => new
                {
                    e.Id,
                    StartDateTime = EF.Property<DateTime>(e, "_startDateTime"),
                    EndDateTime = EF.Property<DateTime>(e, "_endDateTime"),
                    e.CategoryId,
                    EventType = EF.Property<string>(e, "_eventType"),
                    CategoryName = e.Category!.Name,
                })
                .ToListAsync(cancellationToken);

            var registrations = await context.EventRegistrations
                .Where(r => r.RegisteredAt >= fromDate)
                .Select(r => new
                {
                    r.Id,
                    EventId = EF.Property<int>(r, "_eventId"),
                    Status = EF.Property<string>(r, "_status"),
                    r.RegisteredAt,
                })
                .ToListAsync(cancellationToken);

            var now = DateTime.UtcNow;

            var statistics = new EventStatisticsDto
            {
                TotalEvents = events.Count,
                UpcomingEvents = events.Count(e => e.StartDateTime > now),
                OngoingEvents = events.Count(e => e.StartDateTime <= now && e.EndDateTime >= now),
                CompletedEvents = events.Count(e => e.EndDateTime < now),
                TotalRegistrations = registrations.Count,
                ActiveRegistrations = registrations.Count(r => r.Status.Equals("Registered")),
                CancelledRegistrations = registrations.Count(r => r.Status.Equals("Cancelled")),
                EventsByCategory = events.GroupBy(e => e.CategoryName)
                    .ToDictionary(g => g.Key, g => g.Count()),
                EventsByType = events.GroupBy(e => e.EventType)
                    .ToDictionary(g => g.Key, g => g.Count()),
            };

            // Calculate average attendance rate - FIXED
            var completedEventIds = events
                .Where(e => e.EndDateTime < now)
                .Select(e => e.Id)
                .ToList();

            if (completedEventIds.Any())
            {
                var attendanceData = await context.EventRegistrations
                    .Where(r => completedEventIds.Contains(EF.Property<int>(r, "_eventId")) &&
                               (EF.Property<string>(r, "_status") == "Attended" || EF.Property<string>(r, "_status") == "NoShow"))
                    .GroupBy(r => EF.Property<int>(r, "_eventId"))
                    .Select(g => new
                    {
                        EventId = g.Key,
                        TotalRegistrations = g.Count(),
                        AttendedCount = g.Count(r => EF.Property<string>(r, "_status") == "Attended"),
                    })
                    .ToListAsync(cancellationToken);

                if (attendanceData.Any())
                {
                    statistics.AverageAttendanceRate = attendanceData
                        .Average(a => (double)a.AttendedCount / a.TotalRegistrations * 100);
                }
            }

            return statistics;
        }

        public static async Task<List<EventSummaryDto>> GetEventSummariesAsync(
            this ApplicationDbContext context,
            int? categoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool upcomingOnly = false,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = context.Events.AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == categoryId.Value);
            }

            // Use backing fields - FIXED
            if (startDate.HasValue)
            {
                query = query.Where(e => EF.Property<DateTime>(e, "_startDateTime") >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => EF.Property<DateTime>(e, "_startDateTime") <= endDate.Value);
            }

            if (upcomingOnly)
            {
                query = query.Where(e => EF.Property<DateTime>(e, "_startDateTime") > DateTime.UtcNow);
            }

            return await query
                .Include(e => e.Category)
                .Include(e => e.Images.Where(i => i.IsPrimary))
                .Select(e => new EventSummaryDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartDateTime = EF.Property<DateTime>(e, "_startDateTime"),
                    Venue = EF.Property<string>(e, "_venue"),
                    Capacity = EF.Property<int>(e, "_capacity"),
                    CurrentRegistrations = e.Registrations.Count(r => EF.Property<string>(r, "_status") == "Registered"),
                    CategoryName = e.Category!.Name,
                    EventType = EF.Property<string>(e, "_eventType"),
                    IsRegistrationOpen = EF.Property<DateTime>(e, "_startDateTime").AddHours(-2) > DateTime.UtcNow,
                    PrimaryImageUrl = e.Images.FirstOrDefault(i => i.IsPrimary) != null
                        ? e.Images.First(i => i.IsPrimary).FilePath
                        : null,
                })
                .OrderBy(e => EF.Property<DateTime>(e, "_startDateTime"))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public static async Task<List<RegistrationSummaryDto>> GetRegistrationSummariesAsync(
            this ApplicationDbContext context,
            int? eventId = null,
            int? userId = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = context.EventRegistrations.AsQueryable();

            // Use backing fields - FIXED
            if (eventId.HasValue)
            {
                query = query.Where(r => EF.Property<int>(r, "_eventId") == eventId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(r => EF.Property<int>(r, "_userId") == userId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => EF.Property<string>(r, "_status") == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.RegisteredAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(r => r.RegisteredAt <= toDate.Value);
            }

            return await query
                .Include(r => r.Event)
                .Include(r => r.User)
                .Select(r => new RegistrationSummaryDto
                {
                    Id = r.Id,
                    EventId = EF.Property<int>(r, "_eventId"),
                    UserId = EF.Property<int>(r, "_userId"),
                    EventTitle = r.Event!.Title,
                    UserFullName = r.User!.FirstName + " " + r.User.LastName,
                    UserEmail = EF.Property<string>(r.User, "_email"),
                    RegisteredAt = r.RegisteredAt,
                    Status = EF.Property<string>(r, "_status"),
                    CancelledAt = r.CancelledAt,
                })
                .OrderByDescending(r => r.RegisteredAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        public static async Task<Dictionary<string, object>> GetDashboardDataAsync(
            this ApplicationDbContext context,
            CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var last30Days = now.AddDays(-30);

            var data = new Dictionary<string, object>
            {
                ["totalEvents"] = await context.Events.CountAsync(cancellationToken),
                ["upcomingEvents"] = await context.Events
                    .CountAsync(e => EF.Property<DateTime>(e, "_startDateTime") > now, cancellationToken),
                ["totalUsers"] = await context.Users.CountAsync(cancellationToken),
                ["totalRegistrations"] = await context.EventRegistrations.CountAsync(cancellationToken),
                ["recentRegistrations"] = await context.EventRegistrations
                    .CountAsync(r => r.RegisteredAt >= last30Days, cancellationToken),
                ["popularCategories"] = await context.EventCategories
                    .Include(c => c.Events)
                    .Where(c => c.IsActive)
                    .OrderByDescending(c => c.Events.Count)
                    .Take(5)
                    .Select(c => new { c.Name, EventCount = c.Events.Count })
                    .ToListAsync(cancellationToken),
                ["recentEvents"] = await context.Events
                    .Include(e => e.Category)
                    .Where(e => e.CreatedAt >= last30Days)
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(5)
                    .Select(e => new
                    {
                        e.Id,
                        e.Title,
                        StartDateTime = EF.Property<DateTime>(e, "_startDateTime"),
                        CategoryName = e.Category!.Name,
                    })
                    .ToListAsync(cancellationToken),
            };

            return data;
        }

        public static async Task<List<Event>> GetEventsNearingCapacityAsync(
            this ApplicationDbContext context,
            double thresholdPercentage = 0.8,
            CancellationToken cancellationToken = default)
        {
            // Load events with registrations into memory first - FIXED
            var eventsWithRegistrations = await context.Events
                .Include(e => e.Category)
                .Include(e => e.Registrations)
                .Where(e => EF.Property<DateTime>(e, "_startDateTime") > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            // Filter in memory
            return eventsWithRegistrations
                .Where(e =>
                {
                    var activeRegistrations = e.Registrations.Count(r => r.Status.Value == "Registered");
                    var capacityValue = e.Capacity.Value;
                    return (double)activeRegistrations / capacityValue >= thresholdPercentage;
                })
                .ToList();
        }

        public static async Task BulkUpdateRegistrationStatusAsync(
            this ApplicationDbContext context,
            IEnumerable<int> registrationIds,
            string newStatus,
            CancellationToken cancellationToken = default)
        {
            var ids = string.Join(",", registrationIds);
            await context.Database.ExecuteSqlRawAsync(
                "UPDATE EventRegistrations SET Status = {0}, UpdatedAt = {1} WHERE Id IN ({2})",
                newStatus,
                DateTime.UtcNow,
                ids,
                cancellationToken);
        }
    }
}
