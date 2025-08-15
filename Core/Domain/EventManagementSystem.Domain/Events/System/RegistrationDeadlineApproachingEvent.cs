// <copyright file="RegistrationDeadlineApproachingEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.System
{
    using EventManagementSystem.Domain.Common;

    public sealed class RegistrationDeadlineApproachingEvent : IDomainEvent
    {
        public RegistrationDeadlineApproachingEvent(
                ValueObjects.EventId eventId,
                string eventTitle,
                DateTime registrationCutoff,
                TimeSpan timeUntilCutoff,
                int currentRegistrations,
                int remainingCapacity)
        {
            this.EventEntityId = eventId;
            this.EventTitle = eventTitle;
            this.RegistrationCutoff = registrationCutoff;
            this.TimeUntilCutoff = timeUntilCutoff;
            this.CurrentRegistrations = currentRegistrations;
            this.RemainingCapacity = remainingCapacity;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public ValueObjects.EventId EventEntityId { get; }

        public string EventTitle { get; }

        public DateTime RegistrationCutoff { get; }

        public TimeSpan TimeUntilCutoff { get; }

        public int CurrentRegistrations { get; }

        public int RemainingCapacity { get; }
    }
}
