// <copyright file="EventStatisticsDto.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.DTOs
{
    public class EventStatisticsDto
    {
        public int TotalEvents { get; set; }

        public int UpcomingEvents { get; set; }

        public int OngoingEvents { get; set; }

        public int CompletedEvents { get; set; }

        public int TotalRegistrations { get; set; }

        public int ActiveRegistrations { get; set; }

        public int CancelledRegistrations { get; set; }

        public double AverageAttendanceRate { get; set; }

        public Dictionary<string, int> EventsByCategory { get; set; } = new ();

        public Dictionary<string, int> EventsByType { get; set; } = new ();
    }
}
