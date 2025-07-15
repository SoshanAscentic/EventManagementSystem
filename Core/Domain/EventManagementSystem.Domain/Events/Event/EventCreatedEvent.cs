// <copyright file="EventCreatedEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.Event
{
    using EventManagementSystem.Domain.Common;

    public sealed class EventCreatedEvent : IDomainEvent
    {
        public EventCreatedEvent(ValueObjects.EventId eventId, string title, DateTime startDateTime, int capacity)
        {
            this.EventEntityId = eventId;
            this.Title = title;
            this.StartDateTime = startDateTime;
            this.Capacity = capacity;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ValueObjects.EventId EventEntityId { get; }

        public string Title { get; }

        public DateTime StartDateTime { get; }

        public int Capacity { get; }
    }
}
