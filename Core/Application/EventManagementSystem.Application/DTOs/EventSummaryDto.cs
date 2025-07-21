// <copyright file="EventSummaryDto.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.DTOs
{
    public class EventSummaryDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime StartDateTime { get; set; }

        public string Venue { get; set; } = string.Empty;

        public int Capacity { get; set; }

        public int CurrentRegistrations { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public string EventType { get; set; } = string.Empty;

        public bool IsRegistrationOpen { get; set; }

        public string? PrimaryImageUrl { get; set; }
    }
}
