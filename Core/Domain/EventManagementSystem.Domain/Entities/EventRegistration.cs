// <copyright file="EventRegistration.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Entities
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.ValueObjects;

    public class EventRegistration : BaseEntity, IAggregateRoot
    {
        // Private Constructor for EF Core
        private EventRegistration()
        {
            this.EventId = null!;
            this.UserId = null!;
            this.Status = null!;
        }

        public RegistrationId RegistrationId => this.Id > 0 ? RegistrationId.Create(this.Id) : RegistrationId.CreateNew();

        public EventId EventId { get; private set; }

        public UserId UserId { get; private set; }

        public DateTime RegisteredAt { get; private set; }

        public DateTime? CancelledAt { get; private set; }

        public RegistrationStatus Status { get; private set; }

        public string? Notes { get; private set; }

        // Navigation Properties
        public Event? Event { get; private set; }

        public User? User { get; private set; }

        // Business Properties
        public bool IsActive => this.Status.IsActive;

        public bool IsCancelled => this.Status.IsCancelled;

        public TimeSpan? RegistrationDuration => this.CancelledAt?.Subtract(this.RegisteredAt);

        // Factory method for creating new registrations
        public static EventRegistration Create(EventId eventId, UserId userId, string? notes = null)
        {
            var registration = new EventRegistration
            {
                EventId = eventId,
                UserId = userId,
                RegisteredAt = DateTime.UtcNow,
                Status = RegistrationStatus.Registered,
                Notes = notes?.Trim(),
            };

            // Raise domain event
            registration.AddDomainEvent(new UserRegisteredForEventEvent(
                userId, eventId, registration.RegistrationId, registration.RegisteredAt));

            return registration;
        }

        // Factory method for restoring from database
        public static EventRegistration Restore(
            int id,
            int eventId,
            int userId,
            DateTime registeredAt,
            DateTime? cancelledAt,
            string status,
            string? notes,
            DateTime createdAt)
        {
            var registration = new EventRegistration
            {
                EventId = EventId.Create(eventId),
                UserId = UserId.Create(userId),
                RegisteredAt = registeredAt,
                CancelledAt = cancelledAt,
                Status = RegistrationStatus.Create(status),
                Notes = notes,
            };

            // Set base entity properties
            typeof(BaseEntity).GetProperty(nameof(Id))?.SetValue(registration, id);
            registration.SetCreatedAt(createdAt);

            return registration;
        }

        // Business Methods
        public void Cancel(string? reason = null)
        {
            if (this.IsCancelled)
            {
                throw new InvalidOperationException("Registration is already cancelled");
            }

            if (this.Status == RegistrationStatus.Attended)
            {
                throw new InvalidOperationException("Cannot cancel registration after attending event");
            }

            this.CancelledAt = DateTime.UtcNow;
            this.Status = RegistrationStatus.Cancelled;

            if (!string.IsNullOrWhiteSpace(reason))
            {
                this.Notes = string.IsNullOrWhiteSpace(this.Notes)
                    ? $"Cancelled: {reason}"
                    : $"{this.Notes}; Cancelled: {reason}";
            }

            this.MarkAsUpdated();

            AddDomainEvent(new RegistrationCancelledEvent(
                this.RegistrationId, this.EventId, this.UserId, this.CancelledAt.Value, reason));
        }

        public void MarkAsAttended()
        {
            if (this.IsCancelled)
            {
                throw new InvalidOperationException("Cannot mark cancelled registration as attended");
            }

            if (this.Status == RegistrationStatus.Attended)
            {
                return; // Already attended
            }

            this.Status = RegistrationStatus.Attended;
            this.MarkAsUpdated();

            AddDomainEvent(new RegistrationAttendedEvent(
                this.RegistrationId, this.EventId, this.UserId, DateTime.UtcNow));
        }

        public void MarkAsNoShow()
        {
            if (this.IsCancelled)
            {
                throw new InvalidOperationException("Cannot mark cancelled registration as no-show");
            }

            if (this.Status == RegistrationStatus.Attended)
            {
                throw new InvalidOperationException("Cannot mark attended registration as no-show");
            }

            this.Status = RegistrationStatus.NoShow;
            this.MarkAsUpdated();

            AddDomainEvent(new RegistrationNoShowEvent(
                this.RegistrationId, this.EventId, this.UserId, DateTime.UtcNow));
        }

        public void AddNotes(string notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
            {
                return;
            }

            var trimmedNotes = notes.Trim();
            this.Notes = string.IsNullOrWhiteSpace(this.Notes)
                ? trimmedNotes
                : $"{this.Notes}; {trimmedNotes}";

            this.MarkAsUpdated();
        }
    }
}
