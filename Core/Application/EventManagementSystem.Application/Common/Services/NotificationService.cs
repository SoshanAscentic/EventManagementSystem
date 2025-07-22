// <copyright file="NotificationService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Services
{
    using EventManagementSystem.Application.Common.Enums;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using Microsoft.Extensions.Logging;

    public class NotificationService : INotificationService
    {
        private readonly IEventRepository eventRepository;
        private readonly IEventRegistrationRepository registrationRepository;
        private readonly IUserRepository userRepository;
        private readonly ILogger<NotificationService> logger;

        public NotificationService(
            IEventRepository eventRepository,
            IEventRegistrationRepository registrationRepository,
            IUserRepository userRepository,
            ILogger<NotificationService> logger)
        {
            this.eventRepository = eventRepository;
            this.registrationRepository = registrationRepository;
            this.userRepository = userRepository;
            this.logger = logger;
        }

        public async Task SendEventCreatedNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Sending event created notification for event: {EventId} - {EventTitle}", eventId, eventTitle);

                // What i need to do to implement actual notification logic:
                // 1. Send emails to subscribers
                // 2. Send push notifications
                // 3. Send SignalR notifications
                // 4. Create in-app notifications

                // For now, just log the notification
                this.logger.LogInformation("Event created notification sent for: {EventTitle}", eventTitle);

                // Example of what you might implement:
                await this.SendInAppNotificationAsync(
                    "New Event Created",
                    $"A new event '{eventTitle}' has been created and is now available for registration.",
                    NotificationType.EventCreated,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send event created notification for event: {EventId}", eventId);
            }
        }

        public async Task SendEventUpdatedNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Sending event updated notification for event: {EventId} - {EventTitle}", eventId, eventTitle);

                // Get all registered users for this event
                var registrations = await this.registrationRepository.GetActiveRegistrationsByEventAsync(
                    Domain.ValueObjects.EventId.Create(eventId),
                    cancellationToken);

                foreach (var registration in registrations)
                {
                    // Send notification to each registered user
                    await this.SendUserNotificationAsync(
                        registration.UserId.Value,
                        "Event Updated",
                        $"The event '{eventTitle}' you're registered for has been updated. Please check the details.",
                        NotificationType.EventUpdated,
                        cancellationToken);
                }

                this.logger.LogInformation(
                    "Event updated notification sent to {Count} registered users for: {EventTitle}",
                    registrations.Count,
                    eventTitle);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send event updated notification for event: {EventId}", eventId);
            }
        }

        public async Task SendEventCancelledNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Sending event cancelled notification for event: {EventId} - {EventTitle}", eventId, eventTitle);

                var registrations = await this.registrationRepository.GetActiveRegistrationsByEventAsync(
                    Domain.ValueObjects.EventId.Create(eventId),
                    cancellationToken);

                foreach (var registration in registrations)
                {
                    await this.SendUserNotificationAsync(
                        registration.UserId.Value,
                        "Event Cancelled",
                        $"Unfortunately, the event '{eventTitle}' has been cancelled. You will be refunded if applicable.",
                        NotificationType.EventCancelled,
                        cancellationToken);
                }

                this.logger.LogInformation(
                    "Event cancelled notification sent to {Count} registered users for: {EventTitle}",
                    registrations.Count,
                    eventTitle);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send event cancelled notification for event: {EventId}", eventId);
            }
        }

        public async Task SendRegistrationConfirmationAsync(int registrationId, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Sending registration confirmation for registration: {RegistrationId}", registrationId);

                var registration = await this.registrationRepository.GetByIdAsync(
                    RegistrationId.Create(registrationId),
                    cancellationToken);

                if (registration?.Event != null && registration.User != null)
                {
                    await this.SendUserNotificationAsync(
                        registration.UserId.Value,
                        "Registration Confirmed",
                        $"Your registration for '{registration.Event.Title}' has been confirmed. Event starts on {registration.Event.EventDateTime.StartDateTime:MMM dd, yyyy 'at' HH:mm}.",
                        NotificationType.RegistrationConfirmed,
                        cancellationToken);

                    // Send confirmation email
                    await this.SendEmailNotificationAsync(
                        registration.User.Email.Value,
                        "Registration Confirmation",
                        $"Dear {registration.User.FullName},\n\nYour registration for '{registration.Event.Title}' has been confirmed.\n\nEvent Details:\nDate: {registration.Event.EventDateTime.StartDateTime:MMM dd, yyyy}\nTime: {registration.Event.EventDateTime.StartDateTime:HH:mm}\nVenue: {registration.Event.Location.Venue}\nAddress: {registration.Event.Location.Address}",
                        cancellationToken);
                }

                this.logger.LogInformation("Registration confirmation sent for registration: {RegistrationId}", registrationId);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send registration confirmation for registration: {RegistrationId}", registrationId);
            }
        }

        public async Task SendRegistrationCancelledAsync(int registrationId, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Sending registration cancelled notification for registration: {RegistrationId}", registrationId);

                var registration = await this.registrationRepository.GetByIdAsync(
                    RegistrationId.Create(registrationId),
                    cancellationToken);

                if (registration?.Event != null && registration.User != null)
                {
                    await this.SendUserNotificationAsync(
                        registration.UserId.Value,
                        "Registration Cancelled",
                        $"Your registration for '{registration.Event.Title}' has been cancelled.",
                        NotificationType.RegistrationCancelled,
                        cancellationToken);
                }

                this.logger.LogInformation("Registration cancelled notification sent for registration: {RegistrationId}", registrationId);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send registration cancelled notification for registration: {RegistrationId}", registrationId);
            }
        }

        public async Task SendEventReminderAsync(int eventId, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Sending event reminder for event: {EventId}", eventId);

                var eventEntity = await this.eventRepository.GetByIdWithRegistrationsAsync(
                    Domain.ValueObjects.EventId.Create(eventId),
                    cancellationToken);

                if (eventEntity != null)
                {
                    var activeRegistrations = eventEntity.Registrations.Where(r => r.IsActive);

                    foreach (var registration in activeRegistrations)
                    {
                        if (registration.User != null)
                        {
                            var timeUntilEvent = eventEntity.EventDateTime.StartDateTime - DateTime.UtcNow;
                            var reminderMessage = timeUntilEvent.TotalHours <= 24
                                ? $"Reminder: '{eventEntity.Title}' starts in {timeUntilEvent.Hours} hours."
                                : $"Reminder: '{eventEntity.Title}' starts in {timeUntilEvent.Days} days.";

                            await this.SendUserNotificationAsync(
                                registration.UserId.Value,
                                "Event Reminder",
                                reminderMessage,
                                NotificationType.EventReminder,
                                cancellationToken);
                        }
                    }

                    this.logger.LogInformation(
                        "Event reminder sent to {Count} registered users for event: {EventTitle}",
                        activeRegistrations.Count(),
                        eventEntity.Title);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send event reminder for event: {EventId}", eventId);
            }
        }

        public async Task SendEventCapacityReachedAsync(int eventId, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Sending event capacity reached notification for event: {EventId}", eventId);

                var eventEntity = await this.eventRepository.GetByIdAsync(Domain.ValueObjects.EventId.Create(eventId), cancellationToken);
                if (eventEntity != null)
                {
                    // Notify administrators
                    await this.SendAdminNotificationAsync(
                        "Event Capacity Reached",
                        $"The event '{eventEntity.Title}' has reached full capacity ({eventEntity.Capacity.Value} registrations).",
                        NotificationType.EventCapacityReached,
                        cancellationToken);

                    this.logger.LogInformation("Event capacity reached notification sent for event: {EventTitle}", eventEntity.Title);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send event capacity reached notification for event: {EventId}", eventId);
            }
        }

        // Private helper methods
        private async Task SendInAppNotificationAsync(
            string title,
            string message,
            NotificationType type,
            CancellationToken cancellationToken)
        {
            // Implementation for in-app notifications
            // This could store notifications in database for users to see in the app
            this.logger.LogDebug("In-app notification: {Title} - {Message}", title, message);
            await Task.CompletedTask;
        }

        private async Task SendUserNotificationAsync(
            int userId,
            string title,
            string message,
            NotificationType type,
            CancellationToken cancellationToken)
        {
            // Implementation for user-specific notifications
            this.logger.LogDebug("User notification to {UserId}: {Title} - {Message}", userId, title, message);
            await Task.CompletedTask;
        }

        private async Task SendEmailNotificationAsync(
            string email,
            string subject,
            string body,
            CancellationToken cancellationToken)
        {
            // Implementation for email notifications
            // This would integrate with email service (SendGrid, SMTP, etc.)
            this.logger.LogDebug("Email notification to {Email}: {Subject}", email, subject);
            await Task.CompletedTask;
        }

        private async Task SendAdminNotificationAsync(
            string title,
            string message,
            NotificationType type,
            CancellationToken cancellationToken)
        {
            // Implementation for admin notifications
            this.logger.LogDebug("Admin notification: {Title} - {Message}", title, message);
            await Task.CompletedTask;
        }
    }
}
