// <copyright file="EventCategory.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Entities
{
    using EventManagementSystem.Domain.Common;

    public class EventCategory : BaseEntity
    {
        // Private constructor for EF Core
        private EventCategory()
        {
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.ColorCode = string.Empty;
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public string ColorCode { get; private set; }

        public bool IsActive { get; private set; }

        // Navigation property
        public ICollection<Event> Events { get; private set; } = new List<Event>();

        // Factory method for creating new categories
        public static EventCategory Create(string name, string description, string colorCode = "#007bff")
        {
            return new EventCategory
            {
                Name = ValidateName(name),
                Description = ValidateDescription(description),
                ColorCode = ValidateColorCode(colorCode),
                IsActive = true,
            };
        }

        // Business Methods
        public void UpdateDetails(string name, string description, string colorCode)
        {
            this.Name = ValidateName(name);
            this.Description = ValidateDescription(description);
            this.ColorCode = ValidateColorCode(colorCode);
            this.MarkAsUpdated();
        }

        public void Activate()
        {
            this.IsActive = true;
            this.MarkAsUpdated();
        }

        public void Deactivate()
        {
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

        private static string ValidateColorCode(string colorCode)
        {
            if (string.IsNullOrWhiteSpace(colorCode))
            {
                return "#007bff"; // Default blue
            }

            var trimmedColor = colorCode.Trim();
            if (!trimmedColor.StartsWith("#") || trimmedColor.Length != 7)
            {
                throw new ArgumentException("Color code must be in hex format (#RRGGBB)", nameof(colorCode));
            }

            return trimmedColor;
        }
    }
}
