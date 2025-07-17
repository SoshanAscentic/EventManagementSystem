// <copyright file="EventCategory.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Entities
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.Events.System;

    public class EventCategory : BaseEntity, IAggregateRoot
    {
        private readonly List<Event> events = new ();

        // Private constructor for EF Core
        private EventCategory()
        {
            this.Name = string.Empty;
            this.Description = string.Empty;
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public bool IsActive { get; private set; } = true;

        // Navigation property
        public IReadOnlyCollection<Event> Events => this.events.AsReadOnly();

        // Factory method
        public static EventCategory Create(string name, string description)
        {
            var category = new EventCategory
            {
                Name = ValidateName(name),
                Description = ValidateDescription(description),
            };

            category.AddDomainEvent(new EventCategoryCreatedEvent(category.Id, category.Name, category.Description));
            return category;
        }

        // Business methods
        public void UpdateDetails(string name, string description)
        {
            this.Name = ValidateName(name);
            this.Description = ValidateDescription(description);
            this.MarkAsUpdated();
        }

        public void Activate()
        {
            this.IsActive = true;
            this.MarkAsUpdated();
        }

        public void Deactivate()
        {
            if (this.events.Any(e => e.IsUpcoming))
            {
                throw new InvalidOperationException("Cannot deactivate category with upcoming events");
            }

            this.IsActive = false;
            this.MarkAsUpdated();
        }

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Category name cannot be null or empty", nameof(name));
            }

            var trimmedName = name.Trim();
            if (trimmedName.Length > 50)
            {
                throw new ArgumentException("Category name cannot exceed 50 characters", nameof(name));
            }

            return trimmedName;
        }

        private static string ValidateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Category description cannot be null or empty", nameof(description));
            }

            var trimmedDescription = description.Trim();
            if (trimmedDescription.Length > 500)
            {
                throw new ArgumentException("Category description cannot exceed 500 characters", nameof(description));
            }

            return trimmedDescription;
        }
    }
}
