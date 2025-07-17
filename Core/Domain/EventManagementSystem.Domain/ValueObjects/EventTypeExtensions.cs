// <copyright file="EventTypeExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.ValueObjects
{
    using EventManagementSystem.Domain.Enums;

    public static class EventTypeExtensions
    {
        public static string GetDisplayName(this EventType eventType)
        {
            return eventType switch
            {
                EventType.Conference => "Conference",
                EventType.Workshop => "Workshop",
                EventType.Seminar => "Seminar",
                EventType.Webinar => "Webinar",
                EventType.Meeting => "Meeting",
                EventType.Training => "Training",
                EventType.Networking => "Networking",
                EventType.Social => "Social",
                EventType.Competition => "Competition",
                EventType.Exhibition => "Exhibition",
                EventType.Performance => "Performance",
                EventType.Festival => "Festival",
                EventType.Sports => "Sports",
                EventType.Charity => "Charity",
                EventType.Other => "Other",
                _ => eventType.ToString()
            };
        }

        public static string GetDescription(this EventType eventType)
        {
            return eventType switch
            {
                EventType.Conference => "Large formal meeting with presentations and discussions",
                EventType.Workshop => "Interactive learning session with hands-on activities",
                EventType.Seminar => "Educational presentation on a specific topic",
                EventType.Webinar => "Online seminar or presentation",
                EventType.Meeting => "Formal gathering for discussion or decision-making",
                EventType.Training => "Structured learning program to develop skills",
                EventType.Networking => "Event focused on building professional connections",
                EventType.Social => "Casual gathering for social interaction",
                EventType.Competition => "Contest or competitive event",
                EventType.Exhibition => "Display of items or achievements",
                EventType.Performance => "Artistic or entertainment presentation",
                EventType.Festival => "Celebration or series of events",
                EventType.Sports => "Athletic competition or sports event",
                EventType.Charity => "Fundraising or charitable event",
                EventType.Other => "Other type of event",
                _ => "Unknown event type"
            };
        }
    }
}
