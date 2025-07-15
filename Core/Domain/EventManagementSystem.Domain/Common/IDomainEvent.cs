// <copyright file="IDomainEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Common
{
    public interface IDomainEvent
    {
        DateTime OccurredOn { get; }

        Guid EventId { get; }
    }
}
