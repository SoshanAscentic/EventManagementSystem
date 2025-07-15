// <copyright file="EventCompletedEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.System
{
    using EventManagementSystem.Domain.Common;

    public sealed class EventCompletedEvent : IDomainEvent
    {
        public EventCompletedEvent(
                ValueObjects.EventId eventId,
                string eventTitle,
                DateTime completedAt,
                int totalRegistrations,
                int attendees,
                int noShows)
        {
            this.EventEntityId = eventId;
            this.EventTitle = eventTitle;
            this.CompletedAt = completedAt;
            this.TotalRegistrations = totalRegistrations;
            this.Attendees = attendees;
            this.NoShows = noShows;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ValueObjects.EventId EventEntityId { get; }

        public string EventTitle { get; }

        public DateTime CompletedAt { get; }

        public int TotalRegistrations { get; }

        public int Attendees { get; }

        public int NoShows { get; }
    }
}
