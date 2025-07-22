// <copyright file="RefreshToken.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Identity.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RevokedAt { get; set; }

        public string? RevokedByIp { get; set; }

        public string? ReplacedByToken { get; set; }

        public string CreatedByIp { get; set; } = string.Empty;

        public int UserId { get; set; }

        public ApplicationUser User { get; set; } = null!;

        // Computed properties
        public bool IsExpired => DateTime.UtcNow >= this.ExpiresAt;

        public bool IsRevoked => this.RevokedAt != null;

        public bool IsActive => !this.IsRevoked && !this.IsExpired;
    }
}
