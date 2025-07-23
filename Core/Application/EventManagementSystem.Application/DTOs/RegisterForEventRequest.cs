// <copyright file="RegisterForEventRequest.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.DTOs
{
    public class RegisterForEventRequest
    {
        public int EventId { get; set; }

        public string? Notes { get; set; }
    }
}
