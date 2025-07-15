// <copyright file="User.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Entities
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.Events.User;
    using EventManagementSystem.Domain.ValueObjects;

    public class User : BaseEntity, IAggregateRoot
    {
        private readonly List<EventRegistration> registrations = new ();

        // Private constructor for EF Core
        private User()
        {
            this.Email = null!; // Will be set by EF Core
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
        }

        // Value Objects
        public UserId UserId => this.Id > 0 ? UserId.Create(this.Id) : UserId.CreateNew();

        public Email Email { get; private set; }

        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        public Phone? Phone { get; private set; }

        // Computed Properties
        public string FullName => $"{this.FirstName} {this.LastName}".Trim();

        public IReadOnlyCollection<EventRegistration> Registrations => this.registrations.AsReadOnly();

        public int ActiveRegistrationsCount => this.registrations.Count(r => r.Status.IsActive);

        // Factory method for creating new users
        public static User Create(string email, string firstName, string lastName, string? phone = null)
        {
            var user = new User
            {
                Email = Email.Create(email),
                FirstName = ValidateAndTrimName(firstName, nameof(firstName)),
                LastName = ValidateAndTrimName(lastName, nameof(lastName)),
                Phone = string.IsNullOrWhiteSpace(phone) ? null : Phone.Create(phone),
            };

            // Raise domain event
            user.AddDomainEvent(new UserCreatedEvent(user.UserId, user.Email, user.FullName));

            return user;
        }

        // Factory method for restoring from database
        public static User Restore(int id, string email, string firstName, string lastName, string? phone, DateTime createdAt)
        {
            var user = new User
            {
                Email = Email.Create(email),
                FirstName = firstName,
                LastName = lastName,
                Phone = string.IsNullOrWhiteSpace(phone) ? null : Phone.Create(phone),
            };

            // Set base entity properties
            typeof(BaseEntity).GetProperty(nameof(Id))?.SetValue(user, id);
            user.SetCreatedAt(createdAt);

            return user;
        }

        // Business Methods
        public void UpdateProfile(string firstName, string lastName, string? phone = null)
        {
            var oldFullName = this.FullName;

            this.FirstName = ValidateAndTrimName(firstName, nameof(firstName));
            this.LastName = ValidateAndTrimName(lastName, nameof(lastName));
            this.Phone = string.IsNullOrWhiteSpace(phone) ? null : Phone.Create(phone);

            this.MarkAsUpdated();

            // Raise domain event if name changed
            if (oldFullName != this.FullName)
            {
                this.AddDomainEvent(new UserProfileUpdatedEvent(this.UserId, this.Email, this.FullName, oldFullName));
            }
        }

        public void UpdateEmail(string newEmail)
        {
            var oldEmail = this.Email;
            this.Email = Email.Create(newEmail);
            this.MarkAsUpdated();

            this.AddDomainEvent(new UserEmailUpdatedEvent(this.UserId, oldEmail, this.Email));
        }

        public bool CanRegisterForEvent(Event eventToRegister)
        {
            // Business rule: User cannot register for the same event twice
            if (this.registrations.Any(r => r.EventId == eventToRegister.EventId && r.Status.IsActive))
            {
                return false;
            }

            // Business rule: User cannot register for past events
            if (!eventToRegister.IsRegistrationOpen)
            {
                return false;
            }

            // Business rule: User cannot register for full events
            if (eventToRegister.IsFull)
            {
                return false;
            }

            return true;
        }

        public EventRegistration RegisterForEvent(Event eventToRegister)
        {
            if (!this.CanRegisterForEvent(eventToRegister))
            {
                throw new InvalidOperationException("User cannot register for this event");
            }

            var registration = EventRegistration.Create(eventToRegister.EventId, this.UserId);
            this.registrations.Add(registration);

            AddDomainEvent(new UserRegisteredForEventEvent(
                this.UserId, eventToRegister.EventId, registration.RegistrationId, DateTime.UtcNow));

            return registration;
        }

        public void CancelRegistration(EventId eventId)
        {
            var registration = this.registrations.FirstOrDefault(r =>
                r.EventId == eventId && r.Status.IsActive);

            if (registration == null)
            {
                throw new InvalidOperationException("No active registration found for this event");
            }

            registration.Cancel();

            AddDomainEvent(new UserCancelledRegistrationEvent(
                this.UserId, eventId, registration.RegistrationId, DateTime.UtcNow));
        }

        private static string ValidateAndTrimName(string name, string paramName)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
            }

            var trimmedName = name.Trim();
            if (trimmedName.Length > 50)
            {
                throw new ArgumentException($"{paramName} cannot exceed 50 characters", paramName);
            }

            return trimmedName;
        }
    }
}
