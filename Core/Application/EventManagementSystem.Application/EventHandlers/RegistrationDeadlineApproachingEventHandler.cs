// <copyright file="RegistrationDeadlineApproachingEventHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.EventHandlers
{
    using EventManagementSystem.Application.Common.Enums;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Events.System;
    using Microsoft.Extensions.Logging;

    public class RegistrationDeadlineApproachingEventHandler : IDomainEventHandler<RegistrationDeadlineApproachingEvent>
    {
        private readonly ISignalRNotificationService signalRService;
        private readonly ILogger<RegistrationDeadlineApproachingEventHandler> logger;

        public RegistrationDeadlineApproachingEventHandler(
            ISignalRNotificationService signalRService,
            ILogger<RegistrationDeadlineApproachingEventHandler> logger)
        {
            this.signalRService = signalRService;
            this.logger = logger;
        }

        public async Task Handle(RegistrationDeadlineApproachingEvent domainEvent, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation(
                "Registration deadline approaching for event {EventId}: {TimeUntilCutoff} remaining",
                domainEvent.EventEntityId.Value,
                domainEvent.TimeUntilCutoff);

            try
            {
                var notification = new NotificationDto
                {
                    Title = "Registration Deadline Approaching",
                    Message = $"Only {domainEvent.TimeUntilCutoff.Days} days left to register for '{domainEvent.EventTitle}'. {domainEvent.RemainingCapacity} spots remaining!",
                    Type = NotificationType.EventReminder,
                    ActionUrl = $"/events/{domainEvent.EventEntityId.Value}",
                    Data = new Dictionary<string, object>
                    {
                        ["eventId"] = domainEvent.EventEntityId.Value,
                        ["eventTitle"] = domainEvent.EventTitle,
                        ["registrationCutoff"] = domainEvent.RegistrationCutoff,
                        ["remainingCapacity"] = domainEvent.RemainingCapacity,
                        ["timeUntilCutoff"] = domainEvent.TimeUntilCutoff,
                    },
                };

                // Send to all users (marketing/awareness)
                await this.signalRService.SendToAllAsync(notification, cancellationToken);

                this.logger.LogInformation(
                    "Registration deadline notification sent for event: {EventTitle}",
                    domainEvent.EventTitle);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Failed to send registration deadline notification for event {EventId}",
                    domainEvent.EventEntityId.Value);
            }
        }
    }
}
