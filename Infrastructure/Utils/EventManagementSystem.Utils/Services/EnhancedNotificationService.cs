// <copyright file="EnhancedNotificationService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Utils.Services
{
    using EventManagementSystem.Application.Common.Enums;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using Microsoft.Extensions.Logging;

    public class EnhancedNotificationService : INotificationService
    {
        private readonly IEventRepository eventRepository;
        private readonly IEventRegistrationRepository registrationRepository;
        private readonly IUserRepository userRepository;
        private readonly ISignalRNotificationService signalRService;
        private readonly ILogger<EnhancedNotificationService> logger;

        public EnhancedNotificationService(
            IEventRepository eventRepository,
            IEventRegistrationRepository registrationRepository,
            IUserRepository userRepository,
            ISignalRNotificationService signalRService,
            ILogger<EnhancedNotificationService> logger)
        {
            this.eventRepository = eventRepository;
            this.registrationRepository = registrationRepository;
            this.userRepository = userRepository;
            this.signalRService = signalRService;
            this.logger = logger;
        }

        public async Task SendEventCreatedNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Sending event created notification for event: {EventId} - {EventTitle}", eventId, eventTitle);

                var eventEntity = await this.eventRepository.GetByIdAsync(Domain.ValueObjects.EventId.Create(eventId), cancellationToken);
                if (eventEntity == null)
                {
                    return;
                }

                var notification = new NotificationDto
                {
                    Title = "New Event Created",
                    Message = $"A new event '{eventTitle}' has been created and is now available for registration.",
                    Type = NotificationType.EventCreated,
                    ActionUrl = $"/events/{eventId}",
                    Data = new Dictionary<string, object>
                    {
                        ["eventId"] = eventId,
                        ["eventTitle"] = eventTitle,
                        ["eventDate"] = eventEntity.EventDateTime.StartDateTime,
                        ["venue"] = eventEntity.Location.Venue,
                    },
                };

                // Send to all users
                await signalRService.SendToAllAsync(notification, cancellationToken);

                logger.LogInformation("Event created notification sent for: {EventTitle}", eventTitle);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send event created notification for event: {EventId}", eventId);
            }
        }

        public async Task SendEventUpdatedNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Sending event updated notification for event: {EventId} - {EventTitle}", eventId, eventTitle);

                var registrations = await registrationRepository.GetActiveRegistrationsByEventAsync(
                    Domain.ValueObjects.EventId.Create(eventId), cancellationToken);

                if (!registrations.Any()) return;

                var eventEntity = await eventRepository.GetByIdAsync(Domain.ValueObjects.EventId.Create(eventId), cancellationToken);
                if (eventEntity == null) return;

                var notification = new NotificationDto
                {
                    Title = "Event Updated",
                    Message = $"The event '{eventTitle}' you're registered for has been updated. Please check the details.",
                    Type = NotificationType.EventUpdated,
                    ActionUrl = $"/events/{eventId}",
                    Data = new Dictionary<string, object>
                    {
                        ["eventId"] = eventId,
                        ["eventTitle"] = eventTitle,
                        ["eventDate"] = eventEntity.EventDateTime.StartDateTime,
                        ["venue"] = eventEntity.Location.Venue
                    }
                };

                // Send to registered users
                var userIds = registrations.Select(r => r.UserId.Value).ToList();
                await signalRService.SendToUsersAsync(userIds, notification, cancellationToken);

                // Also send to event participants group
                await signalRService.SendToEventParticipantsAsync(eventId, notification, cancellationToken);

                logger.LogInformation("Event updated notification sent to {Count} users for: {EventTitle}", userIds.Count, eventTitle);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send event updated notification for event: {EventId}", eventId);
            }
        }

        public async Task SendEventCancelledNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Sending event cancelled notification for event: {EventId} - {EventTitle}", eventId, eventTitle);

                var registrations = await registrationRepository.GetActiveRegistrationsByEventAsync(
                    Domain.ValueObjects.EventId.Create(eventId), cancellationToken);

                if (!registrations.Any()) return;

                var notification = new NotificationDto
                {
                    Title = "Event Cancelled",
                    Message = $"Unfortunately, the event '{eventTitle}' has been cancelled. You will be refunded if applicable.",
                    Type = NotificationType.EventCancelled,
                    ActionUrl = $"/events/{eventId}",
                    Data = new Dictionary<string, object>
                    {
                        ["eventId"] = eventId,
                        ["eventTitle"] = eventTitle
                    }
                };

                var userIds = registrations.Select(r => r.UserId.Value).ToList();
                await signalRService.SendToUsersAsync(userIds, notification, cancellationToken);

                logger.LogInformation("Event cancelled notification sent to {Count} users for: {EventTitle}", userIds.Count, eventTitle);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send event cancelled notification for event: {EventId}", eventId);
            }
        }

        public async Task SendRegistrationConfirmationAsync(int registrationId, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Sending registration confirmation for registration: {RegistrationId}", registrationId);

                var registration = await registrationRepository.GetByIdAsync(
                    RegistrationId.Create(registrationId), cancellationToken);

                if (registration?.Event == null || registration.User == null) return;

                var notification = new NotificationDto
                {
                    Title = "Registration Confirmed",
                    Message = $"Your registration for '{registration.Event.Title}' has been confirmed. Event starts on {registration.Event.EventDateTime.StartDateTime:MMM dd, yyyy 'at' HH:mm}.",
                    Type = NotificationType.RegistrationConfirmed,
                    UserId = registration.UserId.Value,
                    UserEmail = registration.User.Email.Value,
                    ActionUrl = $"/my-registrations",
                    Data = new Dictionary<string, object>
                    {
                        ["registrationId"] = registrationId,
                        ["eventId"] = registration.EventId.Value,
                        ["eventTitle"] = registration.Event.Title,
                        ["eventDate"] = registration.Event.EventDateTime.StartDateTime,
                        ["venue"] = registration.Event.Location.Venue
                    }
                };

                await signalRService.SendToUserAsync(registration.UserId.Value, notification, cancellationToken);

                logger.LogInformation("Registration confirmation sent for registration: {RegistrationId}", registrationId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send registration confirmation for registration: {RegistrationId}", registrationId);
            }
        }

        public async Task SendRegistrationCancelledAsync(int registrationId, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Sending registration cancelled notification for registration: {RegistrationId}", registrationId);

                var registration = await registrationRepository.GetByIdAsync(
                    RegistrationId.Create(registrationId), cancellationToken);

                if (registration?.Event == null || registration.User == null) return;

                var notification = new NotificationDto
                {
                    Title = "Registration Cancelled",
                    Message = $"Your registration for '{registration.Event.Title}' has been cancelled.",
                    Type = NotificationType.RegistrationCancelled,
                    UserId = registration.UserId.Value,
                    UserEmail = registration.User.Email.Value,
                    ActionUrl = $"/events/{registration.EventId.Value}",
                    Data = new Dictionary<string, object>
                    {
                        ["registrationId"] = registrationId,
                        ["eventId"] = registration.EventId.Value,
                        ["eventTitle"] = registration.Event.Title
                    }
                };

                await signalRService.SendToUserAsync(registration.UserId.Value, notification, cancellationToken);

                logger.LogInformation("Registration cancelled notification sent for registration: {RegistrationId}", registrationId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send registration cancelled notification for registration: {RegistrationId}", registrationId);
            }
        }

        public async Task SendEventReminderAsync(int eventId, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Sending event reminder for event: {EventId}", eventId);

                var eventEntity = await eventRepository.GetByIdWithRegistrationsAsync(
                    Domain.ValueObjects.EventId.Create(eventId), cancellationToken);

                if (eventEntity == null) return;

                var activeRegistrations = eventEntity.Registrations.Where(r => r.IsActive);
                if (!activeRegistrations.Any()) return;

                var timeUntilEvent = eventEntity.EventDateTime.StartDateTime - DateTime.UtcNow;
                var reminderMessage = timeUntilEvent.TotalHours <= 24
                    ? $"Reminder: '{eventEntity.Title}' starts in {timeUntilEvent.Hours} hours."
                    : $"Reminder: '{eventEntity.Title}' starts in {timeUntilEvent.Days} days.";

                var notification = new NotificationDto
                {
                    Title = "Event Reminder",
                    Message = reminderMessage,
                    Type = NotificationType.EventReminder,
                    ActionUrl = $"/events/{eventId}",
                    Data = new Dictionary<string, object>
                    {
                        ["eventId"] = eventId,
                        ["eventTitle"] = eventEntity.Title,
                        ["eventDate"] = eventEntity.EventDateTime.StartDateTime,
                        ["hoursUntilEvent"] = timeUntilEvent.TotalHours,
                        ["venue"] = eventEntity.Location.Venue
                    }
                };

                var userIds = activeRegistrations.Select(r => r.UserId.Value).ToList();
                await signalRService.SendToUsersAsync(userIds, notification, cancellationToken);

                logger.LogInformation("Event reminder sent to {Count} users for event: {EventTitle}", userIds.Count, eventEntity.Title);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send event reminder for event: {EventId}", eventId);
            }
        }

        public async Task SendEventCapacityReachedAsync(int eventId, CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogInformation("Sending event capacity reached notification for event: {EventId}", eventId);

                var eventEntity = await eventRepository.GetByIdAsync(Domain.ValueObjects.EventId.Create(eventId), cancellationToken);
                if (eventEntity == null) return;

                var notification = new NotificationDto
                {
                    Title = "Event Capacity Reached",
                    Message = $"The event '{eventEntity.Title}' has reached full capacity ({eventEntity.Capacity.Value} registrations).",
                    Type = NotificationType.EventCapacityReached,
                    ActionUrl = $"/events/{eventId}",
                    Data = new Dictionary<string, object>
                    {
                        ["eventId"] = eventId,
                        ["eventTitle"] = eventEntity.Title,
                        ["capacity"] = eventEntity.Capacity.Value
                    }
                };

                // Notify administrators
                await signalRService.SendToAdminsAsync(notification, cancellationToken);

                logger.LogInformation("Event capacity reached notification sent for event: {EventTitle}", eventEntity.Title);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send event capacity reached notification for event: {EventId}", eventId);
            }
        }
    }
}
