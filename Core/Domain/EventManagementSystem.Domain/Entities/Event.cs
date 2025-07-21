// <copyright file="Event.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Entities
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.Events.Event;
    using EventManagementSystem.Domain.ValueObjects;

    public class Event : BaseEntity, IAggregateRoot
    {
        private readonly List<EventRegistration> registrations = new();
        private readonly List<EventImage> images = new();

        // Private constructor for EF Core
        private Event()
        {
            this.Title = string.Empty;
            this.Description = string.Empty;
            // Initialize backing fields with default values
            this._startDateTime = DateTime.UtcNow;
            this._endDateTime = DateTime.UtcNow.AddHours(1);
            this._venue = string.Empty;
            this._address = string.Empty;
            this._capacity = 1;
            this._eventType = "Conference";
        }

        // Backing fields for owned type properties
        private DateTime _startDateTime;
        private DateTime _endDateTime;
        private string _venue;
        private string _address;
        private string? _city;
        private string? _country;
        private int _capacity;
        private string _eventType;

        // Properties that EF Core will map directly
        public string Title { get; private set; }
        public string Description { get; private set; }
        public int CategoryId { get; private set; }

        // Owned type properties with proper backing
        public EventDateTime EventDateTime
        {
            get => EventDateTime.CreateForExisting(this._startDateTime, this._endDateTime);
            private set
            {
                this._startDateTime = value.StartDateTime;
                this._endDateTime = value.EndDateTime;
            }
        }

        public EventLocation Location
        {
            get => EventLocation.Create(this._venue, this._address, this._city, this._country);
            private set
            {
                this._venue = value.Venue;
                this._address = value.Address;
                this._city = value.City;
                this._country = value.Country;
            }
        }

        public EventCapacity Capacity
        {
            get => EventCapacity.Create(this._capacity);
            private set => this._capacity = value.Value;
        }

        public EventType EventType
        {
            get => EventType.Create(this._eventType);
            private set => this._eventType = value.Value;
        }

        // Computed Properties
        public EventId EventId => this.Id > 0 ? EventId.Create(this.Id) : EventId.CreateNew();

        // Navigation Properties
        public EventCategory? Category { get; private set; }
        public IReadOnlyCollection<EventRegistration> Registrations => this.registrations.AsReadOnly();
        public IReadOnlyCollection<EventImage> Images => this.images.AsReadOnly();

        // Business Properties
        public int CurrentRegistrations => this.registrations.Count(r => r.Status.IsActive);
        public bool IsFull => this.Capacity.IsFull(this.CurrentRegistrations);
        public int RemainingCapacity => this.Capacity.RemainingCapacity(this.CurrentRegistrations);
        public bool IsRegistrationOpen => this.EventDateTime.IsRegistrationOpen && !this.IsFull;
        public bool IsUpcoming => this.EventDateTime.IsUpcoming;
        public bool IsOngoing => this.EventDateTime.IsOngoing;
        public bool IsCompleted => this.EventDateTime.IsCompleted;
        public EventImage? PrimaryImage => this.images.FirstOrDefault(i => i.IsPrimary) ?? this.images.FirstOrDefault();

        // Factory method for creating new events
        public static Event Create(
            string title,
            string description,
            DateTime startDateTime,
            DateTime endDateTime,
            string venue,
            string address,
            int capacity,
            EventType eventType,
            int categoryId,
            string? city = null,
            string? country = null)
        {
            var eventEntity = new Event
            {
                Title = ValidateTitle(title),
                Description = ValidateDescription(description),
                CategoryId = categoryId,
            };

            // Set value objects through properties
            eventEntity.EventDateTime = EventDateTime.Create(startDateTime, endDateTime);
            eventEntity.Location = EventLocation.Create(venue, address, city, country);
            eventEntity.Capacity = EventCapacity.Create(capacity);
            eventEntity.EventType = eventType;

            // Raise domain event
            eventEntity.AddDomainEvent(new EventCreatedEvent(
                eventEntity.EventId,
                eventEntity.Title,
                eventEntity.EventDateTime.StartDateTime,
                eventEntity.Capacity.Value));

            return eventEntity;
        }

        // Factory method for restoring from database
        public static Event Restore(
            int id,
            string title,
            string description,
            DateTime startDateTime,
            DateTime endDateTime,
            string venue,
            string address,
            string? city,
            string? country,
            int capacity,
            string eventType,
            int categoryId,
            DateTime createdAt)
        {
            var eventEntity = new Event
            {
                Title = title,
                Description = description,
                CategoryId = categoryId,
                _startDateTime = startDateTime,
                _endDateTime = endDateTime,
                _venue = venue,
                _address = address,
                _city = city,
                _country = country,
                _capacity = capacity,
                _eventType = eventType,
            };

            // Set base entity properties
            typeof(BaseEntity).GetProperty(nameof(Id))?.SetValue(eventEntity, id);
            eventEntity.SetCreatedAt(createdAt);

            return eventEntity;
        }

        // Business Methods
        public void UpdateDetails(
            string title,
            string description,
            DateTime startDateTime,
            DateTime endDateTime,
            string venue,
            string address,
            string? city = null,
            string? country = null)
        {
            // Business rule: Cannot update past events
            if (this.IsCompleted)
            {
                throw new InvalidOperationException("Cannot update completed events");
            }

            // Business rule: Cannot update ongoing events significantly
            if (this.IsOngoing)
            {
                throw new InvalidOperationException("Cannot update ongoing events");
            }

            var oldTitle = this.Title;
            var oldDateTime = this.EventDateTime;

            this.Title = ValidateTitle(title);
            this.Description = ValidateDescription(description);
            this.Location = EventLocation.Create(venue, address, city, country);

            // Only allow date changes if no registrations exist or event is far in future
            if (startDateTime != this.EventDateTime.StartDateTime || endDateTime != this.EventDateTime.EndDateTime)
            {
                if (this.CurrentRegistrations > 0 && this.EventDateTime.StartDateTime.Subtract(DateTime.UtcNow).TotalDays < 7)
                {
                    throw new InvalidOperationException("Cannot change event date within 7 days when registrations exist");
                }

                this.EventDateTime = EventDateTime.Create(startDateTime, endDateTime);
            }

            this.MarkAsUpdated();

            this.AddDomainEvent(new EventUpdatedEvent(this.EventId, oldTitle, this.Title, oldDateTime.StartDateTime, this.EventDateTime.StartDateTime));
        }

        public void UpdateCapacity(int newCapacity)
        {
            // Business rule: Cannot reduce capacity below current registrations
            if (newCapacity < this.CurrentRegistrations)
            {
                throw new InvalidOperationException("Cannot reduce capacity below current registrations");
            }

            var oldCapacity = this.Capacity;
            this.Capacity = EventCapacity.Create(newCapacity);
            this.MarkAsUpdated();

            this.AddDomainEvent(new EventCapacityUpdatedEvent(this.EventId, oldCapacity.Value, newCapacity));
        }

        public EventRegistration RegisterUser(UserId userId)
        {
            // Business rule checks
            if (!this.IsRegistrationOpen)
            {
                throw new InvalidOperationException("Registration is not open for this event");
            }

            if (this.IsFull)
            {
                throw new InvalidOperationException("Event is at full capacity");
            }

            // Check for duplicate registration
            if (this.registrations.Any(r => r.UserId == userId && r.Status.IsActive))
            {
                throw new InvalidOperationException("User is already registered for this event");
            }

            var registration = EventRegistration.Create(this.EventId, userId);
            this.registrations.Add(registration);

            // Check if event is now full
            if (this.IsFull)
            {
                this.AddDomainEvent(new EventCapacityReachedEvent(this.EventId, this.Title, this.Capacity.Value));
            }

            return registration;
        }

        public void CancelRegistration(UserId userId)
        {
            var registration = this.registrations.FirstOrDefault(r =>
                r.UserId == userId && r.Status.IsActive);

            if (registration == null)
            {
                throw new InvalidOperationException("No active registration found for this user");
            }

            // Business rule: Cannot cancel within 24 hours of event start
            if (this.EventDateTime.StartDateTime.Subtract(DateTime.UtcNow).TotalHours < 24)
            {
                throw new InvalidOperationException("Cannot cancel registration within 24 hours of event start");
            }

            registration.Cancel();
        }

        public EventImage AddImage(string fileName, string filePath, long fileSize, bool isPrimary = false)
        {
            // Business rule: Only one primary image allowed
            if (isPrimary && this.images.Any(i => i.IsPrimary))
            {
                throw new InvalidOperationException("Event already has a primary image");
            }

            var image = EventImage.Create(this.EventId, fileName, filePath, fileSize, isPrimary);
            this.images.Add(image);

            this.AddDomainEvent(new EventImageAddedEvent(this.EventId, image.Id, fileName, isPrimary));

            return image;
        }

        public void SetPrimaryImage(int imageId)
        {
            var image = this.images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                throw new InvalidOperationException("Image not found");
            }

            // Remove primary flag from other images
            foreach (var img in this.images.Where(i => i.IsPrimary))
            {
                img.SetPrimary(false);
            }

            image.SetPrimary(true);
            this.MarkAsUpdated();
        }

        public void RemoveImage(int imageId)
        {
            var image = this.images.FirstOrDefault(i => i.Id == imageId);
            if (image == null)
            {
                throw new InvalidOperationException("Image not found");
            }

            this.images.Remove(image);
            this.MarkAsUpdated();

            this.AddDomainEvent(new EventImageRemovedEvent(this.EventId, imageId, image.FileName));
        }

        private static string ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Event title cannot be null or empty", nameof(title));
            }

            var trimmedTitle = title.Trim();
            if (trimmedTitle.Length > 200)
            {
                throw new ArgumentException("Event title cannot exceed 200 characters", nameof(title));
            }

            return trimmedTitle;
        }

        private static string ValidateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Event description cannot be null or empty", nameof(description));
            }

            var trimmedDescription = description.Trim();
            if (trimmedDescription.Length > 2000)
            {
                throw new ArgumentException("Event description cannot exceed 2000 characters", nameof(description));
            }

            return trimmedDescription;
        }
    }
}