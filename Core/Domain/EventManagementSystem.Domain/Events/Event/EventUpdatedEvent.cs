// <copyright file="EventUpdatedEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.Event
{
    using EventManagementSystem.Domain.Common;

    public sealed class EventUpdatedEvent : IDomainEvent
    {
        public EventUpdatedEvent(
                ValueObjects.EventId eventId,
                string oldTitle,
                string newTitle,
                DateTime oldStartDateTime,
                DateTime newStartDateTime)
        {
            this.EventEntityId = eventId;
            this.OldTitle = oldTitle;
            this.NewTitle = newTitle;
            this.OldStartDateTime = oldStartDateTime;
            this.NewStartDateTime = newStartDateTime;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ValueObjects.EventId EventEntityId { get; }

        public string OldTitle { get; }

        public string NewTitle { get; }

        public DateTime OldStartDateTime { get; }

        public DateTime NewStartDateTime { get; }
    }
}
