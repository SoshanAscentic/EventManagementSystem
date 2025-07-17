// <copyright file="INotificationService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Interfaces
{
    using System.Threading.Tasks;

    public interface INotificationService
    {
        Task SendEventCreatedNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default);

        Task SendEventUpdatedNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default);

        Task SendEventCancelledNotificationAsync(int eventId, string eventTitle, CancellationToken cancellationToken = default);

        Task SendRegistrationConfirmationAsync(int registrationId, CancellationToken cancellationToken = default);

        Task SendRegistrationCancelledAsync(int registrationId, CancellationToken cancellationToken = default);

        Task SendEventReminderAsync(int eventId, CancellationToken cancellationToken = default);

        Task SendEventCapacityReachedAsync(int eventId, CancellationToken cancellationToken = default);
    }
}
