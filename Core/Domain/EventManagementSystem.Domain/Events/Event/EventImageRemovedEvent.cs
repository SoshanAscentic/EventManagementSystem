// <copyright file="EventImageRemovedEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.Event
{
    using EventManagementSystem.Domain.Common;

    public sealed class EventImageRemovedEvent : IDomainEvent
    {
        public EventImageRemovedEvent(ValueObjects.EventId eventId, int imageId, string fileName)
        {
            this.EventEntityId = eventId;
            this.ImageId = imageId;
            this.FileName = fileName;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ValueObjects.EventId EventEntityId { get; }

        public int ImageId { get; }

        public string FileName { get; }
    }
}
