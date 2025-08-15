// <copyright file="RegistrationAttendedEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.Registration
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.ValueObjects;

    public sealed class RegistrationAttendedEvent : IDomainEvent
    {
        public RegistrationAttendedEvent(
                RegistrationId registrationId,
                ValueObjects.EventId eventId,
                UserId userId,
                DateTime attendedAt)
        {
            this.RegistrationId = registrationId;
            this.EventEntityId = eventId;
            this.UserId = userId;
            this.AttendedAt = attendedAt;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public RegistrationId RegistrationId { get; }

        public ValueObjects.EventId EventEntityId { get; }

        public UserId UserId { get; }

        public DateTime AttendedAt { get; }
    }
}
