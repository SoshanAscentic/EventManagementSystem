// <copyright file="ApplicationRole.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Identity.Entities
{
    using Microsoft.AspNetCore.Identity;

    public class ApplicationRole : IdentityRole<int>
    {
        public ApplicationRole()
                : base()
        {
        }

        public ApplicationRole(string roleName, string? description = null)
            : base(roleName)
        {
            this.Description = description;
        }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}
