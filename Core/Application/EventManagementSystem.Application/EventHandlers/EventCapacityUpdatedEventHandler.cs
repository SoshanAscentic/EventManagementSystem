// <copyright file="EventCapacityUpdatedEventHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.EventHandlers
{
    using EventManagementSystem.Application.Common.Enums;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Events.Event;
    using EventManagementSystem.Domain.Repositories;
    using Microsoft.Extensions.Logging;

    public class EventCapacityUpdatedEventHandler : IDomainEventHandler<EventCapacityUpdatedEvent>
    {
        private readonly ISignalRNotificationService signalRService;
        private readonly IEventRepository eventRepository;
        private readonly ILogger<EventCapacityUpdatedEventHandler> logger;

        public EventCapacityUpdatedEventHandler(
            ISignalRNotificationService signalRService,
            IEventRepository eventRepository,
            ILogger<EventCapacityUpdatedEventHandler> logger)
        {
            this.signalRService = signalRService;
            this.eventRepository = eventRepository;
            this.logger = logger;
        }

        public async Task Handle(EventCapacityUpdatedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation(
                "Event capacity updated for {EventId}: {OldCapacity} -> {NewCapacity}",
                domainEvent.EventEntityId.Value,
                domainEvent.OldCapacity,
                domainEvent.NewCapacity);

            try
            {
                var eventEntity = await eventRepository.GetByIdAsync(domainEvent.EventEntityId, cancellationToken);
                if (eventEntity == null) return;

                var notification = new NotificationDto
                {
                    Title = "Event Capacity Updated",
                    Message = $"Capacity for '{eventEntity.Title}' has been updated from {domainEvent.OldCapacity} to {domainEvent.NewCapacity} spots.",
                    Type = NotificationType.EventUpdated,
                    ActionUrl = $"/events/{domainEvent.EventEntityId.Value}",
                    Data = new Dictionary<string, object>
                    {
                        ["eventId"] = domainEvent.EventEntityId.Value,
                        ["eventTitle"] = eventEntity.Title,
                        ["oldCapacity"] = domainEvent.OldCapacity,
                        ["newCapacity"] = domainEvent.NewCapacity,
                    },
                };

                // Notify admins about capacity changes
                await this.signalRService.SendToAdminsAsync(notification, cancellationToken);

                // If capacity increased, notify all users (more spots available)
                if (domainEvent.NewCapacity > domainEvent.OldCapacity)
                {
                    var publicNotification = new NotificationDto
                    {
                        Title = "More Spots Available",
                        Message = $"Good news! '{eventEntity.Title}' now has more spots available. Register now!",
                        Type = NotificationType.EventUpdated,
                        ActionUrl = $"/events/{domainEvent.EventEntityId.Value}",
                        Data = notification.Data,
                    };

                    await this.signalRService.SendToAllAsync(publicNotification, cancellationToken);
                }

                this.logger.LogInformation("Event capacity update notifications sent for event: {EventTitle}", eventEntity.Title);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Failed to send event capacity update notification for event {EventId}",
                    domainEvent.EventEntityId.Value);
            }
        }
    }
}
