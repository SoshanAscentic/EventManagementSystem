// <copyright file="ISignalRNotificationService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Interfaces
{
    using EventManagementSystem.Application.DTOs;

    public interface ISignalRNotificationService
    {
        Task SendToUserAsync(int userId, NotificationDto notification, CancellationToken cancellationToken = default);

        Task SendToUsersAsync(IEnumerable<int> userIds, NotificationDto notification, CancellationToken cancellationToken = default);

        Task SendToGroupAsync(string groupName, NotificationDto notification, CancellationToken cancellationToken = default);

        Task SendToAllAsync(NotificationDto notification, CancellationToken cancellationToken = default);

        Task SendToAdminsAsync(NotificationDto notification, CancellationToken cancellationToken = default);

        Task SendToEventParticipantsAsync(int eventId, NotificationDto notification, CancellationToken cancellationToken = default);
    }
}
