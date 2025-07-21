// <copyright file="RegistrationSummaryDto.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.DTOs
{
    public class RegistrationSummaryDto
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public int UserId { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public string UserFullName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public DateTime RegisteredAt { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? CancelledAt { get; set; }
    }
}
