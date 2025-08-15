// <copyright file="EventCapacityReachedEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.Event
{
    using EventManagementSystem.Domain.Common;

    public sealed class EventCapacityReachedEvent : IDomainEvent
    {
        public EventCapacityReachedEvent(ValueObjects.EventId eventId, string eventTitle, int capacity)
        {
            this.EventEntityId = eventId;
            this.EventTitle = eventTitle;
            this.Capacity = capacity;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ValueObjects.EventId EventEntityId { get; }

        public string EventTitle { get; }

        public int Capacity { get; }
    }
}
