// <copyright file="UserRegisteredForEventEventHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.EventHandlers
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Events.Registration;
    using Microsoft.Extensions.Logging;

    public class UserRegisteredForEventEventHandler : IDomainEventHandler<UserRegisteredForEventEvent>
    {
        private readonly INotificationService notificationService;
        private readonly ILogger<UserRegisteredForEventEventHandler> logger;

        public UserRegisteredForEventEventHandler(
            INotificationService notificationService,
            ILogger<UserRegisteredForEventEventHandler> logger)
        {
            this.notificationService = notificationService;
            this.logger = logger;
        }

        public async Task Handle(UserRegisteredForEventEvent domainEvent, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation(
                "Handling UserRegisteredForEventEvent for registration {RegistrationId}",
                domainEvent.RegistrationId.Value);

            try
            {
                await this.notificationService.SendRegistrationConfirmationAsync(
                    domainEvent.RegistrationId.Value,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Failed to send registration confirmation for registration {RegistrationId}",
                    domainEvent.RegistrationId.Value);
            }
        }
    }
}
