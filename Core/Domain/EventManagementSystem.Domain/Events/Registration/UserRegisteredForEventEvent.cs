// <copyright file="UserRegisteredForEventEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.Registration
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.ValueObjects;

    public sealed class UserRegisteredForEventEvent : IDomainEvent
    {
        public UserRegisteredForEventEvent(
                UserId userId,
                ValueObjects.EventId eventId,
                RegistrationId registrationId,
                DateTime registeredAt)
        {
            this.UserId = userId;
            this.EventEntityId = eventId;
            this.RegistrationId = registrationId;
            this.RegisteredAt = registeredAt;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public UserId UserId { get; }

        public ValueObjects.EventId EventEntityId { get; }

        public RegistrationId RegistrationId { get; }

        public DateTime RegisteredAt { get; }
    }
}
