// <copyright file="RegistrationCancelledEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.Registration
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.ValueObjects;

    public sealed class RegistrationCancelledEvent : IDomainEvent
    {
        public RegistrationCancelledEvent(
                RegistrationId registrationId,
                ValueObjects.EventId eventId,
                UserId userId,
                DateTime cancelledAt,
                string? reason)
        {
            this.RegistrationId = registrationId;
            this.EventEntityId = eventId;
            this.UserId = userId;
            this.CancelledAt = cancelledAt;
            this.Reason = reason;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public RegistrationId RegistrationId { get; }

        public ValueObjects.EventId EventEntityId { get; }

        public UserId UserId { get; }

        public DateTime CancelledAt { get; }

        public string? Reason { get; }
    }
}
