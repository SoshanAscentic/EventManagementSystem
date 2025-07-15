using EventManagementSystem.Domain.Common;
using EventManagementSystem.Domain.ValueObjects;

namespace EventManagementSystem.Domain.Entities
{
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
                Notes = notes?.Trim()
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
                Notes = notes
            };

            // Set base entity properties
            typeof(BaseEntity).GetProperty(nameof(Id))?.SetValue(registration, id);
            registration.SetCreatedAt(createdAt);

            return registration;
        }

        // Business Methods
        public void Cancel(string? reason = null)
        {
            if (IsCancelled)
                throw new InvalidOperationException("Registration is already cancelled");

            if (Status == RegistrationStatus.Attended)
                throw new InvalidOperationException("Cannot cancel registration after attending event");

            CancelledAt = DateTime.UtcNow;
            Status = RegistrationStatus.Cancelled;

            if (!string.IsNullOrWhiteSpace(reason))
            {
                Notes = string.IsNullOrWhiteSpace(Notes)
                    ? $"Cancelled: {reason}"
                    : $"{Notes}; Cancelled: {reason}";
            }

            MarkAsUpdated();

            AddDomainEvent(new RegistrationCancelledEvent(
                RegistrationId, EventId, UserId, CancelledAt.Value, reason));
        }

        public void MarkAsAttended()
        {
            if (IsCancelled)
                throw new InvalidOperationException("Cannot mark cancelled registration as attended");

            if (Status == RegistrationStatus.Attended)
                return; // Already attended

            Status = RegistrationStatus.Attended;
            MarkAsUpdated();

            AddDomainEvent(new RegistrationAttendedEvent(
                RegistrationId, EventId, UserId, DateTime.UtcNow));
        }

        public void MarkAsNoShow()
        {
            if (IsCancelled)
                throw new InvalidOperationException("Cannot mark cancelled registration as no-show");

            if (Status == RegistrationStatus.Attended)
                throw new InvalidOperationException("Cannot mark attended registration as no-show");

            Status = RegistrationStatus.NoShow;
            MarkAsUpdated();

            AddDomainEvent(new RegistrationNoShowEvent(
                RegistrationId, EventId, UserId, DateTime.UtcNow));
        }

        public void AddNotes(string notes)
        {
            if (string.IsNullOrWhiteSpace(notes))
                return;

            var trimmedNotes = notes.Trim();
            Notes = string.IsNullOrWhiteSpace(Notes)
                ? trimmedNotes
                : $"{Notes}; {trimmedNotes}";

            MarkAsUpdated();
        }
    }
}
