// <copyright file="NotificationDto.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.DTOs
{
    using EventManagementSystem.Application.Common.Enums;

    public class NotificationDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Dictionary<string, object> Data { get; set; } = new ();

        public string? ActionUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public int? UserId { get; set; }

        public string? UserEmail { get; set; }
    }
}
