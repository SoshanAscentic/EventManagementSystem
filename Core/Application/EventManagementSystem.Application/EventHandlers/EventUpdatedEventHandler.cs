// <copyright file="EventUpdatedEventHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.EventHandlers
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Events.Event;
    using Microsoft.Extensions.Logging;

    public class EventUpdatedEventHandler : IDomainEventHandler<EventUpdatedEvent>
    {
        private readonly INotificationService notificationService;
        private readonly ILogger<EventUpdatedEventHandler> logger;

        public EventUpdatedEventHandler(
            INotificationService notificationService,
            ILogger<EventUpdatedEventHandler> logger)
        {
            this.notificationService = notificationService;
            this.logger = logger;
        }

        public async Task Handle(EventUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation(
                "Handling EventUpdatedEvent for event {EventId}: {OldTitle} -> {NewTitle}",
                domainEvent.EventEntityId.Value,
                domainEvent.OldTitle,
                domainEvent.NewTitle);

            try
            {
                // Use the new title for notification
                await this.notificationService.SendEventUpdatedNotificationAsync(
                    domainEvent.EventEntityId.Value,
                    domainEvent.NewTitle,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Failed to send event updated notification for event {EventId}",
                    domainEvent.EventEntityId.Value);
            }
        }
    }
}
