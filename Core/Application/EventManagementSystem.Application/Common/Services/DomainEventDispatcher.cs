// <copyright file="DomainEventDispatcher.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Services
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Common;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<DomainEventDispatcher> logger;

        public DomainEventDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventDispatcher> logger)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            var eventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);

            var handlers = this.serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                try
                {
                    var handleMethod = handlerType.GetMethod("Handle");
                    if (handleMethod != null)
                    {
                        await (Task)handleMethod.Invoke(handler, new object[] { domainEvent, cancellationToken }) !;
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error handling domain event {EventType}", eventType.Name);
                }
            }
        }
    }
}
