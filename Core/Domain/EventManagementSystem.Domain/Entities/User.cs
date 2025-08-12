// <copyright file="User.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Entities
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.Events.Registration;
    using EventManagementSystem.Domain.Events.User;
    using EventManagementSystem.Domain.ValueObjects;

    public class User : BaseEntity, IAggregateRoot
    {
        private readonly List<EventRegistration> registrations = new ();

        // Backing fields for value objects
        private string _email;
        private string? _phone;

        // Private constructor for EF Core
        private User()
        {
            this._email = string.Empty;
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
        }

        // Properties that EF Core will map directly
        public string FirstName { get; private set; }

        public string LastName { get; private set; }

        // Value object properties with backing fields
        public Email Email
        {
            get => Email.Create(this._email);
            private set => this._email = value.Value;
        }

        public Phone? Phone
        {
            get => string.IsNullOrEmpty(this._phone) ? null : Phone.Create(this._phone);
            private set => this._phone = value?.Value;
        }

        // Computed Properties
        public UserId UserId => this.Id > 0 ? UserId.Create(this.Id) : UserId.CreateNew();

        public string FullName => $"{this.FirstName} {this.LastName}".Trim();

        public IReadOnlyCollection<EventRegistration> Registrations => this.registrations.AsReadOnly();

        public int ActiveRegistrationsCount => this.registrations.Count(r => r.Status.IsActive);

        // Factory method for creating new users
        public static User Create(string email, string firstName, string lastName, string? phone = null)
        {
            var user = new User
            {
                FirstName = ValidateAndTrimName(firstName, nameof(firstName)),
                LastName = ValidateAndTrimName(lastName, nameof(lastName)),
            };

            // Set value objects through properties
            user.Email = Email.Create(email);
            user.Phone = string.IsNullOrWhiteSpace(phone) ? null : Phone.Create(phone);

            // Raise domain event
            user.AddDomainEvent(new UserCreatedEvent(user.UserId, user.Email, user.FullName));

            return user;
        }

        // Factory method for restoring from database
        public static User Restore(int id, string email, string firstName, string lastName, string? phone, DateTime createdAt)
        {
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                _email = email,
                _phone = phone,
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

            this.AddDomainEvent(new UserRegisteredForEventEvent(
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

            this.AddDomainEvent(new UserCancelledRegistrationEvent(
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
