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
                await hubContext.Clients.Group($"User_{userId}")
                    .SendAsync("ReceiveNotification", notification, cancellationToken);

                logger.LogInformation("Sent notification {NotificationId} to user {UserId}", notification.Id, userId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send notification {NotificationId} to user {UserId}", notification.Id, userId);
            }
        }

        public async Task SendToUsersAsync(IEnumerable<int> userIds, NotificationDto notification, CancellationToken cancellationToken = default)
        {
            var tasks = userIds.Select(userId => SendToUserAsync(userId, notification, cancellationToken));
            await Task.WhenAll(tasks);
        }

        public async Task SendToGroupAsync(string groupName, NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.Clients.Group(groupName)
                    .SendAsync("ReceiveNotification", notification, cancellationToken);

                logger.LogInformation("Sent notification {NotificationId} to group {GroupName}", notification.Id, groupName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send notification {NotificationId} to group {GroupName}", notification.Id, groupName);
            }
        }

        public async Task SendToAllAsync(NotificationDto notification, CancellationToken cancellationToken = default)
        {
            try
            {
                await hubContext.Clients.All
                    .SendAsync("ReceiveNotification", notification, cancellationToken);

                logger.LogInformation("Sent notification {NotificationId} to all connected users", notification.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send notification {NotificationId} to all users", notification.Id);
            }
        }

        public async Task SendToAdminsAsync(NotificationDto notification, CancellationToken cancellationToken = default)
        {
            await SendToGroupAsync("Admins", notification, cancellationToken);
        }

        public async Task SendToEventParticipantsAsync(int eventId, NotificationDto notification, CancellationToken cancellationToken = default)
        {
            await SendToGroupAsync($"Event_{eventId}", notification, cancellationToken);
        }
    }
}
