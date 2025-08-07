// <copyright file="NotificationBackgroundService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Utils.Services
{
    using EventManagementSystem.Application.Common.Enums;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.Events.System;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class NotificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<NotificationBackgroundService> logger;

        public NotificationBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<NotificationBackgroundService> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Notification Background Service started");

            // Run every 15 minutes
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));

            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = this.serviceProvider.CreateScope();

                    // Existing methods
                    await this.SendEventReminders(scope, stoppingToken);
                    await this.SendRegistrationDeadlineWarnings(scope, stoppingToken);
                    await this.SendPostEventFollowUps(scope, stoppingToken);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error in notification background service");
                }
            }
        }

        private async Task SendEventReminders(IServiceScope scope, CancellationToken cancellationToken)
        {
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                // Get events starting in 24 hours
                var upcomingEvents24h = await this.GetUpcomingEventsInTimeWindow(eventRepository, TimeSpan.FromHours(24), cancellationToken);

                // Filter for events actually starting in 23-25 hour window to avoid duplicates
                var now = DateTime.UtcNow;
                var filtered24h = upcomingEvents24h.Where(e =>
                    e.EventDateTime.StartDateTime >= now.AddHours(23) &&
                    e.EventDateTime.StartDateTime <= now.AddHours(25));

                foreach (var eventEntity in filtered24h)
                {
                    this.logger.LogInformation("Sending 24-hour reminder for event: {EventId} - {EventTitle}", eventEntity.Id, eventEntity.Title);
                    await notificationService.SendEventReminderAsync(eventEntity.Id, cancellationToken);
                }

                // Get events starting in 2 hours
                var upcomingEvents2h = await this.GetUpcomingEventsInTimeWindow(eventRepository, TimeSpan.FromHours(2), cancellationToken);

                // Filter for events actually starting in 1.5-2.5 hour window
                var filtered2h = upcomingEvents2h.Where(e =>
                    e.EventDateTime.StartDateTime >= now.AddHours(1.5) &&
                    e.EventDateTime.StartDateTime <= now.AddHours(2.5));

                foreach (var eventEntity in filtered2h)
                {
                    this.logger.LogInformation("Sending 2-hour reminder for event: {EventId} - {EventTitle}", eventEntity.Id, eventEntity.Title);
                    await notificationService.SendEventReminderAsync(eventEntity.Id, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error sending event reminders");
            }
        }

        private async Task SendRegistrationDeadlineWarnings(IServiceScope scope, CancellationToken cancellationToken)
        {
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
            var signalRService = scope.ServiceProvider.GetRequiredService<ISignalRNotificationService>();
            var domainEventDispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

            try
            {
                // Get events in the next 3 days (for registration deadline warnings)
                var now = DateTime.UtcNow;
                var eventsIn3Days = await this.GetEventsInDateRange(eventRepository, now.AddDays(2.5), now.AddDays(3.5), cancellationToken);

                foreach (var eventEntity in eventsIn3Days)
                {
                    // Assuming the event has registration open until start time if no specific cutoff
                    var registrationCutoff = eventEntity.EventDateTime.StartDateTime.AddDays(-1); // 1 day before event
                    var remainingCapacity = eventEntity.Capacity.Value - eventEntity.CurrentRegistrations;

                    if (remainingCapacity > 0) // Only send if spots available
                    {
                        var deadlineEvent = new RegistrationDeadlineApproachingEvent(
                            Domain.ValueObjects.EventId.Create(eventEntity.Id),
                            eventEntity.Title,
                            registrationCutoff,
                            registrationCutoff - now,
                            eventEntity.CurrentRegistrations,
                            remainingCapacity);

                        await domainEventDispatcher.DispatchAsync(deadlineEvent, cancellationToken);

                        this.logger.LogInformation(
                            "Sent registration deadline warning for event: {EventId} - {EventTitle}",
                            eventEntity.Id,
                            eventEntity.Title);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error sending registration deadline warnings");
            }
        }

        private async Task SendPostEventFollowUps(IServiceScope scope, CancellationToken cancellationToken)
        {
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
            var signalRService = scope.ServiceProvider.GetRequiredService<ISignalRNotificationService>();

            try
            {
                // Get events that ended 1-3 hours ago (for feedback requests)
                var now = DateTime.UtcNow;
                var recentlyEndedEvents = await this.GetEventsInDateRange(eventRepository, now.AddHours(-3), now.AddHours(-1), cancellationToken);

                // Filter for events that actually ended in this window
                var endedEvents = recentlyEndedEvents.Where(e =>
                    e.EventDateTime.EndDateTime >= now.AddHours(-3) &&
                    e.EventDateTime.EndDateTime <= now.AddHours(-1) &&
                    e.Registrations?.Any(r => r.Status == RegistrationStatus.Attended) == true);

                foreach (var eventEntity in endedEvents)
                {
                    var notification = new NotificationDto
                    {
                        Id = Guid.NewGuid().ToString(),
                        Title = "How was the event?",
                        Message = $"Thanks for attending '{eventEntity.Title}'! We'd love your feedback.",
                        Type = NotificationType.EventReminder, // Could add EventFeedback type
                        ActionUrl = $"/events/{eventEntity.Id}/feedback",
                        CreatedAt = DateTime.UtcNow,
                        Data = new Dictionary<string, object>
                        {
                            ["eventId"] = eventEntity.Id,
                            ["eventTitle"] = eventEntity.Title,
                            ["type"] = "feedback_request",
                        },
                    };

                    var attendeeIds = eventEntity.Registrations
                        .Where(r => r.Status == RegistrationStatus.Attended)
                        .Select(r => r.UserId.Value)
                        .ToList();

                    if (attendeeIds.Any())
                    {
                        await signalRService.SendToUsersAsync(attendeeIds, notification, cancellationToken);
                        this.logger.LogInformation(
                            "Sent feedback request to {Count} attendees for event: {EventTitle}",
                            attendeeIds.Count,
                            eventEntity.Title);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error sending post-event follow-ups");
            }
        }

        // Helper methods using your existing repository interface
        private async Task<IReadOnlyList<Event>> GetUpcomingEventsInTimeWindow(
            IEventRepository eventRepository,
            TimeSpan timeWindow,
            CancellationToken cancellationToken)
        {
            try
            {
                // Use your existing GetEventsStartingSoonAsync method
                return await eventRepository.GetEventsStartingSoonAsync(timeWindow, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting upcoming events in time window: {TimeWindow}", timeWindow);
                return new List<Event>();
            }
        }

        private async Task<IReadOnlyList<Event>> GetEventsInDateRange(
            IEventRepository eventRepository,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken)
        {
            try
            {
                // Use your existing GetEventsByDateRangeAsync method
                return await eventRepository.GetEventsByDateRangeAsync(startDate, endDate, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting events in date range: {StartDate} - {EndDate}", startDate, endDate);
                return new List<Event>();
            }
        }
    }
}
