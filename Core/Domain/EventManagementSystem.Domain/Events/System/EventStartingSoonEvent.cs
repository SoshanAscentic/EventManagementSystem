// <copyright file="EventStartingSoonEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.System
{
    using EventManagementSystem.Domain.Common;

    public sealed class EventStartingSoonEvent : IDomainEvent
    {
        public EventStartingSoonEvent(
                ValueObjects.EventId eventId,
                string eventTitle,
                DateTime startDateTime,
                TimeSpan timeUntilStart,
                int registeredUsers)
        {
            this.EventEntityId = eventId;
            this.EventTitle = eventTitle;
            this.StartDateTime = startDateTime;
            this.TimeUntilStart = timeUntilStart;
            this.RegisteredUsers = registeredUsers;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ValueObjects.EventId EventEntityId { get; }

        public string EventTitle { get; }

        public DateTime StartDateTime { get; }

        public TimeSpan TimeUntilStart { get; }

        public int RegisteredUsers { get; }
    }
}
