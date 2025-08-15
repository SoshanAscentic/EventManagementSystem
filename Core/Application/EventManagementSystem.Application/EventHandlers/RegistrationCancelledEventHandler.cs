// <copyright file="RegistrationCancelledEventHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.EventHandlers
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Events.Registration;
    using Microsoft.Extensions.Logging;

    public class RegistrationCancelledEventHandler : IDomainEventHandler<RegistrationCancelledEvent>
    {
        private readonly INotificationService notificationService;
        private readonly ILogger<RegistrationCancelledEventHandler> logger;

        public RegistrationCancelledEventHandler(
            INotificationService notificationService,
            ILogger<RegistrationCancelledEventHandler> logger)
        {
            this.notificationService = notificationService;
            this.logger = logger;
        }

        public async Task Handle(RegistrationCancelledEvent domainEvent, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation(
                "Handling RegistrationCancelledEvent for registration {RegistrationId}",
                domainEvent.RegistrationId.Value);

            try
            {
                await this.notificationService.SendRegistrationCancelledAsync(
                    domainEvent.RegistrationId.Value,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Failed to send registration cancelled notification for registration {RegistrationId}",
                    domainEvent.RegistrationId.Value);
            }
        }
    }
}
