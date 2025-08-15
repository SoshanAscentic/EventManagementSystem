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
                    Title = "New Event Available",
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

                // Send to ALL users (public announcement)
                await this.signalRService.SendToAllAsync(notification, cancellationToken);
                this.logger.LogInformation("Event created notification sent to all users for: {EventTitle}", eventTitle);

                // Also send specifically to admins
                await this.signalRService.SendToAdminsAsync(notification, cancellationToken);
                this.logger.LogInformation("Event created notification sent to admins");
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

                var eventEntity = await this.eventRepository.GetByIdAsync(Domain.ValueObjects.EventId.Create(eventId), cancellationToken);
                if (eventEntity == null)
                {
                    return;
                }

                var notification = new NotificationDto
                {
                    Title = "Event Updated",
                    Message = $"The event '{eventTitle}' has been updated. Please check the details.",
                    Type = NotificationType.EventUpdated,
                    ActionUrl = $"/events/{eventId}",
                    Data = new Dictionary<string, object>
                    {
                        ["eventId"] = eventId,
                        ["eventTitle"] = eventTitle,
                        ["eventDate"] = eventEntity.EventDateTime.StartDateTime,
                        ["venue"] = eventEntity.Location.Venue,
                    },
                };

                // Send to registered users
                var registrations = await this.registrationRepository.GetActiveRegistrationsByEventAsync(
                    Domain.ValueObjects.EventId.Create(eventId), cancellationToken);

                if (registrations.Any())
                {
                    var userIds = registrations.Select(r => r.UserId.Value).ToList();
                    await this.signalRService.SendToUsersAsync(userIds, notification, cancellationToken);
                    this.logger.LogInformation("Event updated notification sent to {Count} registered users", userIds.Count);
                }

                // ALSO send to admins for awareness
                await this.signalRService.SendToAdminsAsync(notification, cancellationToken);
                this.logger.LogInformation("Event updated notification sent to admins");

                // Send to event participants group
                await this.signalRService.SendToEventParticipantsAsync(eventId, notification, cancellationToken);
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
                this.logger.LogInformation("Sending basic event cancelled notification for event: {EventId} - {EventTitle}", eventId, eventTitle);

                // For basic method, try to get registrations, but don't fail if event is deleted
                List<int> userIds = new List<int>();

                try
                {
                    var registrations = await this.registrationRepository.GetActiveRegistrationsByEventAsync(
                        Domain.ValueObjects.EventId.Create(eventId), cancellationToken);
                    userIds = registrations.Select(r => r.UserId.Value).ToList();
                }
                catch (Exception ex)
                {
                    // Event might be deleted already, just log and continue
                    this.logger.LogWarning(ex, "Could not fetch registrations for event {EventId} - event may be deleted", eventId);
                }

                // Send notifications using the enhanced method
                await this.SendEventDeletionNotificationsWithData(eventId, eventTitle, userIds, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send event cancelled notification for event: {EventId}", eventId);
            }
        }

        // Keep the new enhanced method as-is
        public async Task SendEventDeletionNotificationsWithData(
            int eventId,
            string eventTitle,
            List<int> registeredUserIds,
            CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Sending comprehensive event deletion notifications for: {EventTitle}", eventTitle);

                // Send to registered users (if any)
                if (registeredUserIds.Any())
                {
                    var registeredUserNotification = new NotificationDto
                    {
                        Title = "Event Cancelled",
                        Message = $"Unfortunately, the event '{eventTitle}' has been cancelled. You will be refunded if applicable.",
                        Type = NotificationType.EventCancelled,
                        ActionUrl = $"/my-registrations", // Redirect to their registrations page since event no longer exists
                        Data = new Dictionary<string, object>
                        {
                            ["eventId"] = eventId,
                            ["eventTitle"] = eventTitle,
                            ["reason"] = "Event was cancelled by organizer",
                            ["affectedUsers"] = registeredUserIds.Count,
                        },
                    };

                    await this.signalRService.SendToUsersAsync(registeredUserIds, registeredUserNotification, cancellationToken);
                    this.logger.LogInformation(
                        "Event cancellation notification sent to {Count} registered users for: {EventTitle}",
                        registeredUserIds.Count,
                        eventTitle);
                }

                // Send to all admins (always, regardless of registrations)
                var adminNotification = new NotificationDto
                {
                    Title = "Event Deleted - Admin Alert",
                    Message = registeredUserIds.Any()
                        ? $"Event '{eventTitle}' has been deleted. {registeredUserIds.Count} registered users were notified."
                        : $"Event '{eventTitle}' has been deleted. No users were registered.",
                    Type = NotificationType.EventCancelled,
                    ActionUrl = "/admin/events", // Redirect to admin events page
                    Data = new Dictionary<string, object>
                    {
                        ["eventId"] = eventId,
                        ["eventTitle"] = eventTitle,
                        ["affectedUsers"] = registeredUserIds.Count,
                        ["adminAction"] = true,
                    },
                };

                await this.signalRService.SendToAdminsAsync(adminNotification, cancellationToken);
                this.logger.LogInformation("Admin deletion notification sent for: {EventTitle}", eventTitle);

                // Send public notification to all users (optional - you can disable this if too noisy)
                var publicNotification = new NotificationDto
                {
                    Title = "Event Cancelled",
                    Message = $"The event '{eventTitle}' has been cancelled and is no longer available.",
                    Type = NotificationType.EventCancelled,
                    ActionUrl = "/events", // Redirect to events page
                    Data = new Dictionary<string, object>
                    {
                        ["eventId"] = eventId,
                        ["eventTitle"] = eventTitle,
                        ["publicAnnouncement"] = true,
                    },
                };

                // Only send public notification if there were registered users (indicates it was a public event)
                if (registeredUserIds.Count > 0)
                {
                    await this.signalRService.SendToAllAsync(publicNotification, cancellationToken);
                    this.logger.LogInformation("Public event cancellation notification sent for: {EventTitle}", eventTitle);
                }

                // Send browser notifications for registered users (if permissions granted)
                if (registeredUserIds.Any())
                {
                    // This would show desktop notifications for registered users
                    var browserNotification = new NotificationDto
                    {
                        Title = "🚨 Event Cancelled",
                        Message = $"'{eventTitle}' has been cancelled. Check your email for details.",
                        Type = NotificationType.EventCancelled,
                        Data = new Dictionary<string, object>
                        {
                            ["eventId"] = eventId,
                            ["eventTitle"] = eventTitle,
                            ["priority"] = "high",
                            ["showBrowserNotification"] = true,
                        },
                    };

                    await this.signalRService.SendToUsersAsync(registeredUserIds, browserNotification, cancellationToken);
                    this.logger.LogInformation("High-priority browser notifications sent to registered users for: {EventTitle}", eventTitle);
                }

                this.logger.LogInformation("All event deletion notifications completed successfully for: {EventTitle}", eventTitle);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send comprehensive event deletion notifications for: {EventTitle}", eventTitle);
                throw; // Re-throw to let caller handle
            }
        }

        public async Task SendRegistrationConfirmationAsync(int registrationId, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Sending enhanced registration confirmation for registration: {RegistrationId}", registrationId);

                var registration = await this.registrationRepository.GetByIdAsync(
                    RegistrationId.Create(registrationId), cancellationToken);

                if (registration?.Event == null || registration.User == null)
                {
                    return;
                }

                var eventEntity = registration.Event;
                var user = registration.User;

                // 1. Send confirmation to the user who registered
                var userNotification = new NotificationDto
                {
                    Title = "Registration Confirmed",
                    Message = $"Your registration for '{eventEntity.Title}' has been confirmed. Event starts on {eventEntity.EventDateTime.StartDateTime:MMM dd, yyyy 'at' HH:mm}.",
                    Type = NotificationType.RegistrationConfirmed,
                    UserId = user.UserId.Value,
                    UserEmail = user.Email.Value,
                    ActionUrl = $"/my-registrations",
                    Data = new Dictionary<string, object>
                    {
                        ["registrationId"] = registrationId,
                        ["eventId"] = eventEntity.EventId.Value,
                        ["eventTitle"] = eventEntity.Title,
                        ["eventDate"] = eventEntity.EventDateTime.StartDateTime,
                        ["venue"] = eventEntity.Location.Venue,
                        ["userName"] = user.FullName,
                    },
                };

                await this.signalRService.SendToUserAsync(user.UserId.Value, userNotification, cancellationToken);

                // 2. Send notification to admins about new registration
                var adminNotification = new NotificationDto
                {
                    Title = "New Event Registration",
                    Message = $"{user.FullName} has registered for '{eventEntity.Title}'.",
                    Type = NotificationType.RegistrationConfirmed,
                    ActionUrl = $"/admin/events/{eventEntity.EventId.Value}/registrations",
                    Data = new Dictionary<string, object>
                    {
                        ["registrationId"] = registrationId,
                        ["eventId"] = eventEntity.EventId.Value,
                        ["eventTitle"] = eventEntity.Title,
                        ["userName"] = user.FullName,
                        ["userEmail"] = user.Email.Value,
                        ["currentRegistrations"] = eventEntity.CurrentRegistrations,
                        ["capacity"] = eventEntity.Capacity.Value,
                    },
                };

                await this.signalRService.SendToAdminsAsync(adminNotification, cancellationToken);

                // Notify other participants about new member (if event has multiple participants)
                var otherRegistrations = await this.registrationRepository.GetActiveRegistrationsByEventAsync(
                    eventEntity.EventId, cancellationToken);

                if (otherRegistrations.Count > 1) // More than just the new registrant
                {
                    var communityNotification = new NotificationDto
                    {
                        Title = "New Participant Joined",
                        Message = $"Someone new has joined '{eventEntity.Title}'. {eventEntity.CurrentRegistrations} people are now registered!",
                        Type = NotificationType.RegistrationConfirmed,
                        ActionUrl = $"/events/{eventEntity.EventId.Value}",
                        Data = new Dictionary<string, object>
                        {
                            ["eventId"] = eventEntity.EventId.Value,
                            ["eventTitle"] = eventEntity.Title,
                            ["currentRegistrations"] = eventEntity.CurrentRegistrations,
                            ["capacity"] = eventEntity.Capacity.Value,
                        },
                    };

                    // Send to other registered users (exclude the new registrant)
                    var otherUserIds = otherRegistrations
                        .Where(r => r.UserId.Value != user.UserId.Value)
                        .Select(r => r.UserId.Value)
                        .ToList();

                    if (otherUserIds.Any())
                    {
                        await this.signalRService.SendToUsersAsync(otherUserIds, communityNotification, cancellationToken);
                        this.logger.LogInformation("Community registration notification sent to {Count} other participants", otherUserIds.Count);
                    }
                }

                // Check for milestone notifications
                await this.CheckAndSendMilestoneNotifications(eventEntity.EventId.Value, eventEntity.CurrentRegistrations, eventEntity.Capacity.Value, cancellationToken);

                // Check for high demand alerts
                await this.CheckAndSendHighDemandAlert(eventEntity.EventId.Value, eventEntity.Title, cancellationToken);

                this.logger.LogInformation("Enhanced registration confirmation sent for registration: {RegistrationId}", registrationId);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send enhanced registration confirmation for registration: {RegistrationId}", registrationId);
            }
        }

        public async Task SendRegistrationCancelledAsync(int registrationId, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Sending enhanced registration cancelled notification for registration: {RegistrationId}", registrationId);

                var registration = await this.registrationRepository.GetByIdAsync(
                    RegistrationId.Create(registrationId), cancellationToken);

                if (registration?.Event == null || registration.User == null)
                {
                    return;
                }

                var eventEntity = registration.Event;
                var user = registration.User;

                // Send cancellation notification to the user
                var userNotification = new NotificationDto
                {
                    Title = "Registration Cancelled",
                    Message = $"Your registration for '{eventEntity.Title}' has been cancelled.",
                    Type = NotificationType.RegistrationCancelled,
                    UserId = user.UserId.Value,
                    UserEmail = user.Email.Value,
                    ActionUrl = $"/events/{eventEntity.EventId.Value}",
                    Data = new Dictionary<string, object>
                    {
                        ["registrationId"] = registrationId,
                        ["eventId"] = eventEntity.EventId.Value,
                        ["eventTitle"] = eventEntity.Title,
                        ["userName"] = user.FullName,
                    },
                };

                await this.signalRService.SendToUserAsync(user.UserId.Value, userNotification, cancellationToken);

                // Send notification to admins about cancelled registration
                var adminNotification = new NotificationDto
                {
                    Title = "Registration Cancelled",
                    Message = $"{user.FullName} has cancelled their registration for '{eventEntity.Title}'.",
                    Type = NotificationType.RegistrationCancelled,
                    ActionUrl = $"/admin/events/{eventEntity.EventId.Value}/registrations",
                    Data = new Dictionary<string, object>
                    {
                        ["registrationId"] = registrationId,
                        ["eventId"] = eventEntity.EventId.Value,
                        ["eventTitle"] = eventEntity.Title,
                        ["userName"] = user.FullName,
                        ["userEmail"] = user.Email.Value,
                        ["currentRegistrations"] = Math.Max(0, eventEntity.CurrentRegistrations - 1),
                        ["capacity"] = eventEntity.Capacity.Value,
                    },
                };

                await this.signalRService.SendToAdminsAsync(adminNotification, cancellationToken);

                // Check if we should notify about spot availability
                var currentRegistrations = eventEntity.CurrentRegistrations - 1; // After cancellation
                var capacity = eventEntity.Capacity.Value;
                var capacityRatio = (double)eventEntity.CurrentRegistrations / capacity;

                // If event was previously at high capacity (70%+), notify others about available spot
                if (capacityRatio >= 0.7 && currentRegistrations < capacity)
                {
                    var availabilityNotification = new NotificationDto
                    {
                        Title = "Spot Available",
                        Message = $"A spot has opened up for '{eventEntity.Title}'. Register now if you're interested!",
                        Type = NotificationType.EventReminder,
                        ActionUrl = $"/events/{eventEntity.EventId.Value}",
                        Data = new Dictionary<string, object>
                        {
                            ["eventId"] = eventEntity.EventId.Value,
                            ["eventTitle"] = eventEntity.Title,
                            ["currentRegistrations"] = currentRegistrations,
                            ["capacity"] = capacity,
                            ["spotsAvailable"] = capacity - currentRegistrations,
                        },
                    };

                    // Send to all users (public announcement of availability)
                    await this.signalRService.SendToAllAsync(availabilityNotification, cancellationToken);
                    this.logger.LogInformation("Spot availability notification sent for event: {EventTitle}", eventEntity.Title);
                }

                // Notify other registered participants about the cancellation
                var otherRegistrations = await this.registrationRepository.GetActiveRegistrationsByEventAsync(
                    eventEntity.EventId, cancellationToken);

                if (otherRegistrations.Any())
                {
                    var communityNotification = new NotificationDto
                    {
                        Title = "Participant Left Event",
                        Message = $"Someone has cancelled their registration for '{eventEntity.Title}'. {currentRegistrations} people are now registered.",
                        Type = NotificationType.RegistrationCancelled,
                        ActionUrl = $"/events/{eventEntity.EventId.Value}",
                        Data = new Dictionary<string, object>
                        {
                            ["eventId"] = eventEntity.EventId.Value,
                            ["eventTitle"] = eventEntity.Title,
                            ["currentRegistrations"] = currentRegistrations,
                            ["capacity"] = capacity,
                        },
                    };

                    var otherUserIds = otherRegistrations
                        .Where(r => r.UserId.Value != user.UserId.Value) // Exclude the cancelled user
                        .Select(r => r.UserId.Value)
                        .ToList();

                    if (otherUserIds.Any())
                    {
                        await this.signalRService.SendToUsersAsync(otherUserIds, communityNotification, cancellationToken);
                        this.logger.LogInformation("Community cancellation notification sent to {Count} other participants", otherUserIds.Count);
                    }
                }

                this.logger.LogInformation("Enhanced registration cancelled notification sent for registration: {RegistrationId}", registrationId);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send enhanced registration cancelled notification for registration: {RegistrationId}", registrationId);
            }
        }

        public async Task SendEventReminderAsync(int eventId, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Sending event reminder for event: {EventId}", eventId);

                var eventEntity = await this.eventRepository.GetByIdWithRegistrationsAsync(
                    Domain.ValueObjects.EventId.Create(eventId), cancellationToken);

                if (eventEntity == null)
                {
                    return;
                }

                var activeRegistrations = eventEntity.Registrations.Where(r => r.IsActive);
                if (!activeRegistrations.Any())
                {
                    return;
                }

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
                        ["venue"] = eventEntity.Location.Venue,
                    },
                };

                var userIds = activeRegistrations.Select(r => r.UserId.Value).ToList();
                await this.signalRService.SendToUsersAsync(userIds, notification, cancellationToken);

                this.logger.LogInformation("Event reminder sent to {Count} users for event: {EventTitle}", userIds.Count, eventEntity.Title);
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
                if (eventEntity == null)
                {
                    return;
                }

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
                        ["capacity"] = eventEntity.Capacity.Value,
                    },
                };

                // Notify administrators
                await this.signalRService.SendToAdminsAsync(notification, cancellationToken);

                this.logger.LogInformation("Event capacity reached notification sent for event: {EventTitle}", eventEntity.Title);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send event capacity reached notification for event: {EventId}", eventId);
            }
        }

        private async Task CheckAndSendMilestoneNotifications(int eventId, int currentRegistrations, int capacity, CancellationToken cancellationToken)
        {
            try
            {
                var capacityRatio = (double)currentRegistrations / capacity;
                string? milestoneMessage = null;
                NotificationType notificationType = NotificationType.EventReminder;

                // Define milestone thresholds
                if (capacityRatio >= 0.9) // 90% full
                {
                    milestoneMessage = $"Almost full! Only {capacity - currentRegistrations} spots remaining.";
                    notificationType = NotificationType.EventCapacityReached;
                }
                else if (capacityRatio >= 0.75) // 75% full
                {
                    milestoneMessage = $"Filling up fast! {capacity - currentRegistrations} spots remaining.";
                }
                else if (capacityRatio >= 0.5) // 50% full
                {
                    milestoneMessage = $"Halfway full! {currentRegistrations} people registered so far.";
                }
                else if (currentRegistrations == 10 || currentRegistrations == 25 || currentRegistrations == 50)
                {
                    milestoneMessage = $"Great news! {currentRegistrations} people have now registered.";
                }

                if (!string.IsNullOrEmpty(milestoneMessage))
                {
                    var eventEntity = await this.eventRepository.GetByIdAsync(Domain.ValueObjects.EventId.Create(eventId), cancellationToken);
                    if (eventEntity == null)
                    {
                        return;
                    }

                    var milestoneNotification = new NotificationDto
                    {
                        Title = "Registration Milestone",
                        Message = $"'{eventEntity.Title}': {milestoneMessage}",
                        Type = notificationType,
                        ActionUrl = $"/events/{eventId}",
                        Data = new Dictionary<string, object>
                        {
                            ["eventId"] = eventId,
                            ["eventTitle"] = eventEntity.Title,
                            ["currentRegistrations"] = currentRegistrations,
                            ["capacity"] = capacity,
                            ["capacityRatio"] = capacityRatio,
                            ["spotsRemaining"] = capacity - currentRegistrations,
                        },
                    };

                    // Send to admins for management awareness
                    await this.signalRService.SendToAdminsAsync(milestoneNotification, cancellationToken);

                    // If it's a high-interest milestone (75%+ full), notify all users
                    if (capacityRatio >= 0.75)
                    {
                        await this.signalRService.SendToAllAsync(milestoneNotification, cancellationToken);
                    }

                    this.logger.LogInformation(
                        "Registration milestone notification sent for event: {EventTitle} ({CurrentRegistrations}/{Capacity})",
                        eventEntity.Title,
                        currentRegistrations,
                        capacity);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send milestone notifications for event: {EventId}", eventId);
            }
        }

        private async Task CheckAndSendHighDemandAlert(int eventId, string eventTitle, CancellationToken cancellationToken)
        {
            try
            {
                // Get registrations from last 24 hours
                var yesterday = DateTime.UtcNow.AddDays(-1);
                var recentRegistrations = await this.registrationRepository.GetRegistrationsByDateRangeAsync(yesterday, DateTime.UtcNow, cancellationToken);
                var eventRecentRegistrations = recentRegistrations.Where(r => r.EventId.Value == eventId).Count();

                var eventEntity = await this.eventRepository.GetByIdAsync(Domain.ValueObjects.EventId.Create(eventId), cancellationToken);
                if (eventEntity == null)
                {
                    return;
                }

                // If more than 10 registrations in 24 hours, or more than 25% of capacity filled recently
                var capacityThreshold = Math.Max(10, eventEntity.Capacity.Value * 0.25);

                if (eventRecentRegistrations >= capacityThreshold)
                {
                    var alertNotification = new NotificationDto
                    {
                        Title = "High Demand Alert",
                        Message = $"'{eventTitle}' is experiencing high demand with {eventRecentRegistrations} registrations in the last 24 hours!",
                        Type = NotificationType.EventCapacityReached,
                        ActionUrl = $"/events/{eventId}",
                        Data = new Dictionary<string, object>
                        {
                            ["eventId"] = eventId,
                            ["eventTitle"] = eventTitle,
                            ["recentRegistrations"] = eventRecentRegistrations,
                            ["currentRegistrations"] = eventEntity.CurrentRegistrations,
                            ["capacity"] = eventEntity.Capacity.Value,
                            ["timeframe"] = "24 hours",
                        },
                    };

                    // Notify admins about high demand
                    await this.signalRService.SendToAdminsAsync(alertNotification, cancellationToken);

                    // Optionally notify all users about popular event
                    var publicNotification = new NotificationDto
                    {
                        Title = "Trending Event",
                        Message = $"'{eventTitle}' is trending! Don't miss out - register now.",
                        Type = NotificationType.EventReminder,
                        ActionUrl = $"/events/{eventId}",
                        Data = alertNotification.Data,
                    };

                    await this.signalRService.SendToAllAsync(publicNotification, cancellationToken);

                    this.logger.LogInformation(
                        "High demand alert sent for event: {EventTitle} ({RecentRegistrations} recent registrations)",
                        eventTitle,
                        eventRecentRegistrations);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send high demand alert for event: {EventId}", eventId);
            }
        }
    }
}
