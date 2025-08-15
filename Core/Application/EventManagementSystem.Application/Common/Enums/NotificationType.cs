// <copyright file="NotificationType.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Enums
{
    public enum NotificationType
    {
        // Basic types
        Info,
        Success,
        Warning,
        Error,

        // Event-specific types
        EventCreated,
        EventUpdated,
        EventCancelled,
        EventReminder,
        EventCapacityReached,

        // Registration types
        RegistrationConfirmed,
        RegistrationCancelled,
        RegistrationReminder,
        RegistrationMilestone,

        // Additional types to match frontend
        MoreSpotsAvailable,
        LiveEventUpdate,
        SpotAvailable,
        HighDemand,
    }
}
