// <copyright file="JwtSettings.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Identity.Configuration
{
    public class JwtSettings
    {
        public static readonly string SectionName = "JwtSettings";

        public string Secret { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public int ExpiryInMinutes { get; set; } = 15;

        public int RefreshTokenExpiryInDays { get; set; } = 7;
    }
}
