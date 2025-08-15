// <copyright file="SignalRNotificationService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Utils.Services
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.DTOs;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;

    public class SignalRNotificationService : ISignalRNotificationService
    {
        private readonly IHubContext<NotificationHub> hubContext;
        private readonly ILogger<SignalRNotificationService> logger;

        public SignalRNotificationService(
            IHubContext<NotificationHub> hubContext,
            ILogger<SignalRNotificationService> logger)
        {
            this.hubContext = hubContext;
            this.logger = logger;
        }

        public async Task SendToUserAsync(int userId, NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                // Send to specific user group
                await this.hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("ReceiveNotification", notification, cancellationToken);

                this.logger.LogInformation(
                    "Sent notification {NotificationId} to user {UserId}: {Title}",
                    notification.Id,
                    userId,
                    notification.Title);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Failed to send notification {NotificationId} to user {UserId}",
                    notification.Id,
                    userId);
            }
        }

        public async Task SendToUsersAsync(IEnumerable<int> userIds, NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                var userIdList = userIds.ToList();
                this.logger.LogInformation(
                    "Sending notification {NotificationId} to {UserCount} users: {Title}",
                    notification.Id,
                    userIdList.Count,
                    notification.Title);

                var tasks = userIdList.Select(userId => this.SendToUserAsync(userId, notification, cancellationToken));
                await Task.WhenAll(tasks);

                this.logger.LogInformation(
                    "Successfully sent notification {NotificationId} to {UserCount} users",
                    notification.Id,
                    userIdList.Count);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send notification {NotificationId} to multiple users", notification.Id);
            }
        }

        public async Task SendToGroupAsync(string groupName, NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                await this.hubContext.Clients.Group(groupName)
                    .SendAsync("ReceiveNotification", notification, cancellationToken);

                this.logger.LogInformation(
                    "Sent notification {NotificationId} to group {GroupName}: {Title}",
                    notification.Id,
                    groupName,
                    notification.Title);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Failed to send notification {NotificationId} to group {GroupName}",
                    notification.Id,
                    groupName);
            }
        }

        public async Task SendToAllAsync(NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                // Send to AllUsers group instead of All to ensure only authenticated users receive it
                await this.hubContext.Clients.Group("AllUsers")
                    .SendAsync("ReceiveNotification", notification, cancellationToken);

                this.logger.LogInformation(
                    "Sent notification {NotificationId} to all connected users: {Title}",
                    notification.Id,
                    notification.Title);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send notification {NotificationId} to all users", notification.Id);
            }
        }

        public async Task SendToAdminsAsync(NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                await this.SendToGroupAsync("Admins", notification, cancellationToken);
                this.logger.LogInformation(
                    "Sent notification {NotificationId} to admins: {Title}",
                    notification.Id,
                    notification.Title);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send notification {NotificationId} to admins", notification.Id);
            }
        }

        public async Task SendToEventParticipantsAsync(int eventId, NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                await this.SendToGroupAsync($"Event_{eventId}", notification, cancellationToken);
                this.logger.LogInformation(
                    "Sent notification {NotificationId} to event {EventId} participants: {Title}",
                    notification.Id,
                    eventId,
                    notification.Title);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Failed to send notification {NotificationId} to event {EventId} participants",
                    notification.Id,
                    eventId);
            }
        }

        // Additional helper methods for better notification targeting
        public async Task SendToEventManagersAsync(NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                await this.SendToGroupAsync("EventManagers", notification, cancellationToken);
                this.logger.LogInformation(
                    "Sent notification {NotificationId} to event managers: {Title}",
                    notification.Id,
                    notification.Title);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send notification {NotificationId} to event managers", notification.Id);
            }
        }

        public async Task SendToStaffAsync(NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                // Send to both admins and event managers
                await Task.WhenAll(
                    this.SendToAdminsAsync(notification, cancellationToken),
                    this.SendToEventManagersAsync(notification, cancellationToken));

                this.logger.LogInformation(
                    "Sent notification {NotificationId} to all staff: {Title}",
                    notification.Id,
                    notification.Title);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send notification {NotificationId} to staff", notification.Id);
            }
        }
    }
}
