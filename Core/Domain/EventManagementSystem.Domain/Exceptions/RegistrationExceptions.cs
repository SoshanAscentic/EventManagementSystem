// <copyright file="RegistrationExceptions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Exceptions
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.ValueObjects;

    public sealed class RegistrationNotFoundException : DomainException
    {
        public RegistrationNotFoundException(RegistrationId registrationId)
            : base("Registration.NotFound", $"Registration with ID {registrationId.Value} was not found.")
        {
            this.RegistrationId = registrationId;
        }

        public RegistrationNotFoundException(int registrationId)
            : this(RegistrationId.Create(registrationId))
        {
        }

        public RegistrationId RegistrationId { get; }
    }

    public sealed class InvalidRegistrationException : DomainException
    {
        public InvalidRegistrationException(UserId userId, EventId eventId, string reason)
            : base(
                "Registration.Invalid",
                $"Invalid registration for user {userId.Value} and event {eventId.Value}: {reason}")
        {
            this.UserId = userId;
            this.EventId = eventId;
        }

        public UserId UserId { get; }

        public EventId EventId { get; }
    }

    public sealed class DuplicateRegistrationException : DomainException
    {
        public DuplicateRegistrationException(UserId userId, EventId eventId)
            : base(
                "Registration.Duplicate",
                $"User {userId.Value} is already registered for event {eventId.Value}.")
        {
            this.UserId = userId;
            this.EventId = eventId;
        }

        public UserId UserId { get; }

        public EventId EventId { get; }
    }

    public sealed class RegistrationCancellationNotAllowedException : DomainException
    {
        public RegistrationCancellationNotAllowedException(RegistrationId registrationId, string reason)
            : base(
                "Registration.CancellationNotAllowed",
                $"Registration {registrationId.Value} cannot be cancelled: {reason}")
        {
            this.RegistrationId = registrationId;
            this.Reason = reason;
        }

        public RegistrationId RegistrationId { get; }

        public string Reason { get; }
    }

    public sealed class RegistrationAlreadyCancelledException : DomainException
    {
        public RegistrationAlreadyCancelledException(RegistrationId registrationId, DateTime cancelledAt)
            : base(
                "Registration.AlreadyCancelled",
                $"Registration {registrationId.Value} was already cancelled at {cancelledAt:yyyy-MM-dd HH:mm}.")
        {
            this.RegistrationId = registrationId;
            this.CancelledAt = cancelledAt;
        }

        public RegistrationId RegistrationId { get; }

        public DateTime CancelledAt { get; }
    }

    public sealed class RegistrationDeadlinePassedException : DomainException
    {
        public RegistrationDeadlinePassedException(EventId eventId, DateTime deadline, string operation)
            : base(
                "Registration.DeadlinePassed",
                $"Cannot {operation} registration for event {eventId.Value}. Deadline was {deadline:yyyy-MM-dd HH:mm}.")
        {
            this.EventId = eventId;
            this.Deadline = deadline;
        }

        public EventId EventId { get; }

        public DateTime Deadline { get; }
    }
}
