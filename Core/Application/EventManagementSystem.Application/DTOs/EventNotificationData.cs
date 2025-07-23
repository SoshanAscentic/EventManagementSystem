// <copyright file="EventNotificationData.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.DTOs
{
    public class EventNotificationData
    {
        public int EventId { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string? Venue { get; set; }

        public int? Capacity { get; set; }

        public int? CurrentRegistrations { get; set; }
    }
}
