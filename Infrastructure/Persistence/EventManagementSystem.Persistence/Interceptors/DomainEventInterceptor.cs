// <copyright file="DomainEventInterceptor.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Interceptors
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Common;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;

    public class DomainEventInterceptor : SaveChangesInterceptor
    {
        private readonly IDomainEventDispatcher domainEventDispatcher;

        public DomainEventInterceptor(IDomainEventDispatcher domainEventDispatcher)
        {
            this.domainEventDispatcher = domainEventDispatcher;
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context is not null)
            {
                await this.DispatchDomainEventsAsync(eventData.Context, cancellationToken);
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
        {
            var aggregateRoots = context.ChangeTracker.Entries<IAggregateRoot>()
                .Where(e => e.Entity.DomainEvents.Any())
                .Select(e => e.Entity)
                .ToList();

            var domainEvents = aggregateRoots
                .SelectMany(ar => ar.DomainEvents)
                .ToList();

            aggregateRoots.ForEach(ar => ar.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
            {
                await this.domainEventDispatcher.DispatchAsync(domainEvent, cancellationToken);
            }
        }
    }
}
