// <copyright file="TestEndpoints.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Endpoints
{
    using EventManagementSystem.Application.Common.Enums;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.DTOs;

    public static class TestEndpoints
    {
        public static void MapTestEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/test")
                .WithTags("Test SignalR");

            group.MapPost("/send-notification", async (
                ISignalRNotificationService signalRService) =>
            {
                var testNotification = new NotificationDto
                {
                    Title = "Test Notification",
                    Message = "This is a test notification from the API",
                    Type = NotificationType.EventCreated,
                    CreatedAt = DateTime.UtcNow,
                };

                await signalRService.SendToAllAsync(testNotification);

                return Results.Ok("Test notification sent to all users");
            })
            .WithName("SendTestNotification")
            .WithOpenApi();

            group.MapPost("/send-to-user/{userId}", async (
                int userId,
                ISignalRNotificationService signalRService) =>
            {
                var testNotification = new NotificationDto
                {
                    Title = "Personal Test Notification",
                    Message = $"This is a personal test notification for user {userId}",
                    Type = NotificationType.RegistrationConfirmed,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                };

                await signalRService.SendToUserAsync(userId, testNotification);

                return Results.Ok($"Test notification sent to user {userId}");
            })
            .WithName("SendTestNotificationToUser")
            .WithOpenApi();
        }
    }
}
