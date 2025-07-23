// <copyright file="UpdateUserProfileRequest.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.DTOs
{
    public class UpdateUserProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Phone { get; set; }
    }
}
