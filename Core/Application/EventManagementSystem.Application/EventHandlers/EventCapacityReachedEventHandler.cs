// <copyright file="EventCapacityReachedEventHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.EventHandlers
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Events.Event;
    using Microsoft.Extensions.Logging;

    public class EventCapacityReachedEventHandler : IDomainEventHandler<EventCapacityReachedEvent>
    {
        private readonly INotificationService notificationService;
        private readonly ILogger<EventCapacityReachedEventHandler> logger;

        public EventCapacityReachedEventHandler(
            INotificationService notificationService,
            ILogger<EventCapacityReachedEventHandler> logger)
        {
            this.notificationService = notificationService;
            this.logger = logger;
        }

        public async Task Handle(EventCapacityReachedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation(
                "Handling EventCapacityReachedEvent for event {EventId}",
                domainEvent.EventEntityId.Value);

            try
            {
                await this.notificationService.SendEventCapacityReachedAsync(
                    domainEvent.EventEntityId.Value,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Failed to send capacity reached notification for event {EventId}",
                    domainEvent.EventEntityId.Value);
            }
        }
    }
}
