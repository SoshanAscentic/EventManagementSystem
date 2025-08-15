// <copyright file="IntegrationEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.System
{
    using EventManagementSystem.Domain.Common;

    public sealed class IntegrationEvent : IDomainEvent
    {
        public IntegrationEvent(string eventType, object data, Dictionary<string, object>? metadata = null)
        {
            this.EventType = eventType;
            this.Data = data;
            this.Metadata = metadata ?? new Dictionary<string, object>();
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public string EventType { get; }

        public object Data { get; }

        public Dictionary<string, object> Metadata { get; }
    }
}
