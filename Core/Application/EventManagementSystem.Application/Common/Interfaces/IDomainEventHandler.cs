// <copyright file="IDomainEventHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Interfaces
{
    using EventManagementSystem.Domain.Common;

    public interface IDomainEventHandler<in T>
        where T : IDomainEvent
    {
        Task Handle(T domainEvent, CancellationToken cancellationToken = default);
    }
}
