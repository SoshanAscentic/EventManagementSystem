// <copyright file="NotificationHub.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Utils.Services
{
    using System.Security.Claims;
    using EventManagementSystem.Application.Common.Enums;
    using EventManagementSystem.Application.DTOs;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.Logging;

    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            this.logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = this.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        this.Context.User?.FindFirst("sub")?.Value ??
                        this.Context.User?.FindFirst("id")?.Value;

            var userRole = this.Context.User?.FindFirst(ClaimTypes.Role)?.Value ??
                          this.Context.User?.FindFirst("role")?.Value;

            var userEmail = this.Context.User?.FindFirst(ClaimTypes.Email)?.Value ??
                           this.Context.User?.FindFirst("email")?.Value;

            var connectionId = this.Context.ConnectionId;

            this.logger.LogInformation(
                "SignalR: User {UserId} ({Email}) connected with ConnectionId {ConnectionId} and role {Role}",
                userId,
                userEmail,
                connectionId,
                userRole);

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group
                await this.Groups.AddToGroupAsync(connectionId, $"User_{userId}");
                this.logger.LogInformation("SignalR: Added user {UserId} to personal group User_{UserId}", userId, userId);

                // Add to role-based groups with better role handling
                if (!string.IsNullOrEmpty(userRole))
                {
                    var roles = userRole.Split(',').Select(r => r.Trim().ToLowerInvariant());

                    foreach (var role in roles)
                    {
                        switch (role)
                        {
                            case "admin":
                            case "administrator":
                                await this.Groups.AddToGroupAsync(connectionId, "Admins");
                                this.logger.LogInformation("SignalR: Added user {UserId} to Admins group", userId);
                                break;
                            case "eventmanager":
                            case "manager":
                                await this.Groups.AddToGroupAsync(connectionId, "EventManagers");
                                this.logger.LogInformation("SignalR: Added user {UserId} to EventManagers group", userId);
                                break;
                            case "user":
                            default:
                                await this.Groups.AddToGroupAsync(connectionId, "Users");
                                this.logger.LogInformation("SignalR: Added user {UserId} to Users group", userId);
                                break;
                        }
                    }
                }
                else
                {
                    // Default to Users group if no role
                    await this.Groups.AddToGroupAsync(connectionId, "Users");
                    this.logger.LogInformation("SignalR: Added user {UserId} to default Users group", userId);
                }

                // Add to "AllUsers" group for broadcast notifications
                await this.Groups.AddToGroupAsync(connectionId, "AllUsers");
                this.logger.LogInformation("SignalR: Added user {UserId} to AllUsers group", userId);
            }
            else
            {
                this.logger.LogWarning("SignalR: User connected without valid UserId claim");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = this.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        this.Context.User?.FindFirst("sub")?.Value ??
                        this.Context.User?.FindFirst("id")?.Value;

            var userRole = this.Context.User?.FindFirst(ClaimTypes.Role)?.Value ??
                          this.Context.User?.FindFirst("role")?.Value;

            var connectionId = this.Context.ConnectionId;

            this.logger.LogInformation(
                "SignalR: User {UserId} disconnected from ConnectionId {ConnectionId}. Exception: {Exception}",
                userId,
                connectionId,
                exception?.Message);

            if (!string.IsNullOrEmpty(userId))
            {
                await this.Groups.RemoveFromGroupAsync(connectionId, $"User_{userId}");
                await this.Groups.RemoveFromGroupAsync(connectionId, "AllUsers");

                if (!string.IsNullOrEmpty(userRole))
                {
                    var roles = userRole.Split(',').Select(r => r.Trim().ToLowerInvariant());

                    foreach (var role in roles)
                    {
                        switch (role)
                        {
                            case "admin":
                            case "administrator":
                                await this.Groups.RemoveFromGroupAsync(connectionId, "Admins");
                                break;
                            case "eventmanager":
                            case "manager":
                                await this.Groups.RemoveFromGroupAsync(connectionId, "EventManagers");
                                break;
                            case "user":
                            default:
                                await this.Groups.RemoveFromGroupAsync(connectionId, "Users");
                                break;
                        }
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinEventGroup(int eventId)
        {
            var userId = this.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        this.Context.User?.FindFirst("sub")?.Value ??
                        this.Context.User?.FindFirst("id")?.Value;

            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, $"Event_{eventId}");
            this.logger.LogInformation("SignalR: User {UserId} joined event group {EventId}", userId, eventId);
        }

        public async Task LeaveEventGroup(int eventId)
        {
            var userId = this.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        this.Context.User?.FindFirst("sub")?.Value ??
                        this.Context.User?.FindFirst("id")?.Value;

            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, $"Event_{eventId}");
            this.logger.LogInformation("SignalR: User {UserId} left event group {EventId}", userId, eventId);
        }

        public async Task JoinUserGroup()
        {
            var userId = this.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        this.Context.User?.FindFirst("sub")?.Value ??
                        this.Context.User?.FindFirst("id")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await this.Groups.AddToGroupAsync(this.Context.ConnectionId, $"User_{userId}");
                this.logger.LogInformation("SignalR: User {UserId} explicitly joined personal group User_{UserId}", userId, userId);

                // Send confirmation back to the caller
                await this.Clients.Caller.SendAsync("JoinUserGroupConfirmed", new { userId, timestamp = DateTime.UtcNow });
            }
            else
            {
                this.logger.LogWarning("SignalR: JoinUserGroup called but no valid UserId found");
                await this.Clients.Caller.SendAsync("Error", "Unable to join user group: Invalid user ID");
            }
        }

        public async Task TestNotification()
        {
            var userId = this.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        this.Context.User?.FindFirst("sub")?.Value ??
                        this.Context.User?.FindFirst("id")?.Value;

            var userEmail = this.Context.User?.FindFirst(ClaimTypes.Email)?.Value ??
                           this.Context.User?.FindFirst("email")?.Value;

            this.logger.LogInformation("SignalR: Test notification requested by user {UserId}", userId);

            var testNotification = new NotificationDto
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Test Notification from Backend",
                Message = $"This is a test notification sent to user {userEmail} at {DateTime.UtcNow:HH:mm:ss}",
                Type = NotificationType.EventCreated, // Use a safe enum value
                CreatedAt = DateTime.UtcNow,
                UserId = int.TryParse(userId, out var userIdInt) ? userIdInt : null,
                UserEmail = userEmail,
            };

            // Send to the specific user
            await this.Clients.Caller.SendAsync("ReceiveNotification", testNotification);
            this.logger.LogInformation("SignalR: Test notification sent to user {UserId}", userId);
        }

        public async Task SendTestNotification()
        {
            await this.TestNotification();
        }

        public async Task Ping()
        {
            var userId = this.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        this.Context.User?.FindFirst("sub")?.Value ??
                        this.Context.User?.FindFirst("id")?.Value;

            this.logger.LogInformation("SignalR: Ping received from user {UserId}", userId);
            await this.Clients.Caller.SendAsync("Pong", new
            {
                userId,
                timestamp = DateTime.UtcNow,
                connectionId = this.Context.ConnectionId,
            });
        }

        // Broadcast test method (Admin only)
        public async Task BroadcastTestNotification(string message)
        {
            var userId = this.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                        this.Context.User?.FindFirst("sub")?.Value ??
                        this.Context.User?.FindFirst("id")?.Value;

            var userRole = this.Context.User?.FindFirst(ClaimTypes.Role)?.Value ??
                          this.Context.User?.FindFirst("role")?.Value;

            if (!userRole?.ToLowerInvariant().Contains("admin") == true)
            {
                this.logger.LogWarning("SignalR: Non-admin user {UserId} attempted to broadcast", userId);
                await this.Clients.Caller.SendAsync("Error", "Access denied: Admin role required");
                return;
            }

            this.logger.LogInformation("SignalR: Admin {UserId} broadcasting test notification", userId);

            var broadcastNotification = new NotificationDto
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Broadcast Test Notification",
                Message = message ?? $"This is a broadcast test from admin at {DateTime.UtcNow:HH:mm:ss}",
                Type = NotificationType.EventCreated, // Use a safe enum value
                CreatedAt = DateTime.UtcNow,
            };

            // Send to all connected users
            await this.Clients.Group("AllUsers").SendAsync("ReceiveNotification", broadcastNotification);
            this.logger.LogInformation("SignalR: Broadcast notification sent by admin {UserId}", userId);
        }
    }
}
