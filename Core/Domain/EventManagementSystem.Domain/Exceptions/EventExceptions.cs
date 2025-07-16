// <copyright file="EventExceptions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Exceptions
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.ValueObjects;

    public sealed class EventNotFoundException : DomainException
    {
        public EventId EventId { get; }

        public EventNotFoundException(EventId eventId)
            : base("Event.NotFound", $"Event with ID {eventId.Value} was not found.")
        {
            this.EventId = eventId;
        }

        public EventNotFoundException(int eventId)
            : this(EventId.Create(eventId))
        {
        }
    }

    public sealed class EventCapacityExceededException : DomainException
    {
        public EventId EventId { get; }

        public int CurrentCapacity { get; }

        public int MaxCapacity { get; }

        public EventCapacityExceededException(EventId eventId, int currentCapacity, int maxCapacity)
            : base(
                "Event.CapacityExceeded",
                $"Event {eventId.Value} is at full capacity ({currentCapacity}/{maxCapacity}). Cannot register more attendees.")
        {
            this.EventId = eventId;
            this.CurrentCapacity = currentCapacity;
            this.MaxCapacity = maxCapacity;
        }
    }

    public sealed class RegistrationClosedException : DomainException
    {
        public EventId EventId { get; }

        public DateTime RegistrationCutoff { get; }

        public RegistrationClosedException(EventId eventId, DateTime registrationCutoff)
            : base(
                "Event.RegistrationClosed",
                $"Registration for event {eventId.Value} is closed. Registration deadline was {registrationCutoff:yyyy-MM-dd HH:mm}.")
        {
            this.EventId = eventId;
            this.RegistrationCutoff = registrationCutoff;
        }
    }

    public sealed class PastEventException : DomainException
    {
        public EventId EventId { get; }

        public DateTime EventStartTime { get; }

        public PastEventException(EventId eventId, DateTime eventStartTime, string operation)
            : base(
                "Event.PastEvent",
                $"Cannot {operation} for past event {eventId.Value}. Event started on {eventStartTime:yyyy-MM-dd HH:mm}.")
        {
            this.EventId = eventId;
            this.EventStartTime = eventStartTime;
        }
    }

    public sealed class OngoingEventException : DomainException
    {
        public EventId EventId { get; }

        public OngoingEventException(EventId eventId, string operation)
            : base(
                "Event.OngoingEvent",
                $"Cannot {operation} for ongoing event {eventId.Value}.")
        {
            this.EventId = eventId;
        }
    }

    public sealed class EventHasRegistrationsException : DomainException
    {
        public EventId EventId { get; }

        public int ActiveRegistrations { get; }

        public EventHasRegistrationsException(EventId eventId, int activeRegistrations)
            : base(
                "Event.HasActiveRegistrations",
                $"Cannot delete event {eventId.Value} because it has {activeRegistrations} active registrations.")
        {
            this.EventId = eventId;
            this.ActiveRegistrations = activeRegistrations;
        }
    }

    public sealed class InvalidEventDateTimeException : DomainException
    {
        public DateTime StartDateTime { get; }

        public DateTime EndDateTime { get; }

        public InvalidEventDateTimeException(DateTime startDateTime, DateTime endDateTime, string reason)
            : base(
                "Event.InvalidDateTime",
                $"Invalid event date/time: Start {startDateTime:yyyy-MM-dd HH:mm}, End {endDateTime:yyyy-MM-dd HH:mm}. {reason}")
        {
            this.StartDateTime = startDateTime;
            this.EndDateTime = endDateTime;
        }
    }
}
