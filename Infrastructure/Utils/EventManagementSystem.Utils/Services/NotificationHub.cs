// <copyright file="NotificationHub.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Utils.Services
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;

    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = this.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = this.Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                // Add user to their personal group
                await this.Groups.AddToGroupAsync(this.Context.ConnectionId, $"User_{userId}");

                // Add to role-based groups
                if (userRole == "Admin")
                {
                    await this.Groups.AddToGroupAsync(this.Context.ConnectionId, "Admins");
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = this.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = this.Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, $"User_{userId}");

                if (userRole == "Admin")
                {
                    await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, "Admins");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinEventGroup(int eventId)
        {
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, $"Event_{eventId}");
        }

        public async Task LeaveEventGroup(int eventId)
        {
            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, $"Event_{eventId}");
        }
    }
}
