// <copyright file="NotificationHub.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Utils.Services
{
    using EventManagementSystem.Application.Common.Enums;
    using EventManagementSystem.Application.DTOs;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;
    using System.Security.Claims;

    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;
            var connectionId = Context.ConnectionId;

            _logger.LogInformation("SignalR: User {UserId} ({Email}) connected with ConnectionId {ConnectionId} and role {Role}",
                userId, userEmail, connectionId, userRole);

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group
                await Groups.AddToGroupAsync(connectionId, $"User_{userId}");
                _logger.LogInformation("SignalR: Added user {UserId} to personal group User_{UserId}", userId, userId);

                // Add to role-based groups
                if (userRole == "Admin")
                {
                    await Groups.AddToGroupAsync(connectionId, "Admins");
                    _logger.LogInformation("SignalR: Added user {UserId} to Admins group", userId);
                }
            }
            else
            {
                _logger.LogWarning("SignalR: User connected without valid UserId claim");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var connectionId = Context.ConnectionId;

            _logger.LogInformation("SignalR: User {UserId} disconnected from ConnectionId {ConnectionId}. Exception: {Exception}",
                userId, connectionId, exception?.Message);

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(connectionId, $"User_{userId}");

                if (userRole == "Admin")
                {
                    await Groups.RemoveFromGroupAsync(connectionId, "Admins");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinEventGroup(int eventId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Event_{eventId}");
            _logger.LogInformation("SignalR: User {UserId} joined event group {EventId}", userId, eventId);
        }

        public async Task LeaveEventGroup(int eventId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Event_{eventId}");
            _logger.LogInformation("SignalR: User {UserId} left event group {EventId}", userId, eventId);
        }

        public async Task JoinUserGroup()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                _logger.LogInformation("SignalR: User {UserId} explicitly joined personal group User_{UserId}", userId, userId);

                // Send confirmation back to the caller
                await Clients.Caller.SendAsync("JoinUserGroupConfirmed", new { userId, timestamp = DateTime.UtcNow });
            }
            else
            {
                _logger.LogWarning("SignalR: JoinUserGroup called but no valid UserId found");
                await Clients.Caller.SendAsync("Error", "Unable to join user group: Invalid user ID");
            }
        }

        // Helper method to get a safe notification type
        private NotificationType GetSafeNotificationType()
        {
            // Get the first available enum value (usually 0)
            var enumValues = Enum.GetValues(typeof(NotificationType));
            return enumValues.Length > 0 ? (NotificationType)enumValues.GetValue(0) : default(NotificationType);
        }

        // Test methods for debugging
        public async Task TestNotification()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

            _logger.LogInformation("SignalR: Test notification requested by user {UserId}", userId);

            var testNotification = new NotificationDto
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Test Notification from Backend",
                Message = $"This is a test notification sent to user {userEmail} at {DateTime.UtcNow:HH:mm:ss}",
                Type = GetSafeNotificationType(), // Use helper method
                CreatedAt = DateTime.UtcNow,
                UserId = int.TryParse(userId, out var userIdInt) ? userIdInt : null,
                UserEmail = userEmail
            };

            // Send to the specific user
            await Clients.Caller.SendAsync("ReceiveNotification", testNotification);
            _logger.LogInformation("SignalR: Test notification sent to user {UserId}", userId);
        }

        public async Task SendTestNotification()
        {
            await TestNotification(); // Alias for TestNotification
        }

        public async Task Ping()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogInformation("SignalR: Ping received from user {UserId}", userId);
            await Clients.Caller.SendAsync("Pong", new
            {
                userId,
                timestamp = DateTime.UtcNow,
                connectionId = Context.ConnectionId
            });
        }

        // Broadcast test method (Admin only)
        public async Task BroadcastTestNotification(string message)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Admin")
            {
                _logger.LogWarning("SignalR: Non-admin user {UserId} attempted to broadcast", userId);
                await Clients.Caller.SendAsync("Error", "Access denied: Admin role required");
                return;
            }

            _logger.LogInformation("SignalR: Admin {UserId} broadcasting test notification", userId);

            var broadcastNotification = new NotificationDto
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Broadcast Test Notification",
                Message = message ?? $"This is a broadcast test from admin at {DateTime.UtcNow:HH:mm:ss}",
                Type = GetSafeNotificationType(), // Use helper method
                CreatedAt = DateTime.UtcNow
            };

            // Send to all connected users
            await Clients.All.SendAsync("ReceiveNotification", broadcastNotification);
            _logger.LogInformation("SignalR: Broadcast notification sent by admin {UserId}", userId);
        }
    }
}