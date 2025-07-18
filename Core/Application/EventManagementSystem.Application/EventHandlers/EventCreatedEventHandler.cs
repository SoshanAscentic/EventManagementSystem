// <copyright file="EventCreatedEventHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.EventHandlers
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Events.Event;
    using Microsoft.Extensions.Logging;

    public class EventCreatedEventHandler : IDomainEventHandler<EventCreatedEvent>
    {
        private readonly INotificationService notificationService;
        private readonly ILogger<EventCreatedEventHandler> logger;

        public EventCreatedEventHandler(
            INotificationService notificationService,
            ILogger<EventCreatedEventHandler> logger)
        {
            this.notificationService = notificationService;
            this.logger = logger;
        }

        public async Task Handle(EventCreatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation("Handling EventCreatedEvent for event {EventId}", domainEvent.EventEntityId.Value);

            try
            {
                await this.notificationService.SendEventCreatedNotificationAsync(
                    domainEvent.EventEntityId.Value,
                    domainEvent.Title,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send event created notification for event {EventId}", domainEvent.EventEntityId.Value);
            }
        }
    }
}
