// <copyright file="EventCapacityUpdatedEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.Event
{
    using EventManagementSystem.Domain.Common;

    public sealed class EventCapacityUpdatedEvent : IDomainEvent
    {
        public EventCapacityUpdatedEvent(ValueObjects.EventId eventId, int oldCapacity, int newCapacity)
        {
            this.EventEntityId = eventId;
            this.OldCapacity = oldCapacity;
            this.NewCapacity = newCapacity;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ValueObjects.EventId EventEntityId { get; }

        public int OldCapacity { get; }

        public int NewCapacity { get; }
    }
}
