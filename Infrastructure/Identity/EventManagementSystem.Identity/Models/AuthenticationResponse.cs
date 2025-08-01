// <copyright file="AuthenticationResponse.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Identity.Models
{
    public class AuthenticationResponse
    {
        public int UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public IList<string> Roles { get; set; } = new List<string>();

        public string AccessToken { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsEmailConfirmed { get; set; }

        // Add this property
        public string RefreshToken { get; set; } = string.Empty;
    }
}
