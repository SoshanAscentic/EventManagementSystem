// <copyright file="EventImageAddedEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.Event
{
    using EventManagementSystem.Domain.Common;

    public sealed class EventImageAddedEvent : IDomainEvent
    {
        public EventImageAddedEvent(ValueObjects.EventId eventId, int imageId, string fileName, bool isPrimary)
        {
            this.EventEntityId = eventId;
            this.ImageId = imageId;
            this.FileName = fileName;
            this.IsPrimary = isPrimary;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ValueObjects.EventId EventEntityId { get; }

        public int ImageId { get; }

        public string FileName { get; }

        public bool IsPrimary { get; }
    }
}
