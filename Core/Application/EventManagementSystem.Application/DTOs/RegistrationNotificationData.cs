// <copyright file="RegistrationNotificationData.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.DTOs
{
    public class RegistrationNotificationData
    {
        public int RegistrationId { get; set; }

        public int EventId { get; set; }

        public int UserId { get; set; }

        public string EventTitle { get; set; } = string.Empty;

        public string UserName { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }
    }
}
