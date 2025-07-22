// <copyright file="ApplicationUser.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Identity.Entities
{
    using Microsoft.AspNetCore.Identity;

    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        // Navigation to domain user entity
        public int? DomainUserId { get; set; }

        // Computed properties
        public string FullName => $"{this.FirstName} {this.LastName}".Trim();

        // Helper methods for domain value objects (use these in services instead)
        public EventManagementSystem.Domain.ValueObjects.Email GetEmailValueObject()
        {
            return EventManagementSystem.Domain.ValueObjects.Email.Create(this.Email ?? string.Empty);
        }

        public EventManagementSystem.Domain.ValueObjects.Phone? GetPhoneValueObject()
        {
            return string.IsNullOrEmpty(this.Phone) ? null : EventManagementSystem.Domain.ValueObjects.Phone.Create(this.Phone);
        }
    }
}
