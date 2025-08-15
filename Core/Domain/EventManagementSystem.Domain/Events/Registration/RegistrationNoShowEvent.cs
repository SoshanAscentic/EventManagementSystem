// <copyright file="RegistrationNoShowEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.Registration
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.ValueObjects;

    public sealed class RegistrationNoShowEvent : IDomainEvent
    {
        public RegistrationNoShowEvent(
                RegistrationId registrationId,
                ValueObjects.EventId eventId,
                UserId userId,
                DateTime markedAt)
        {
            this.RegistrationId = registrationId;
            this.EventEntityId = eventId;
            this.UserId = userId;
            this.MarkedAt = markedAt;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public RegistrationId RegistrationId { get; }

        public ValueObjects.EventId EventEntityId { get; }

        public UserId UserId { get; }

        public DateTime MarkedAt { get; }
    }
}
