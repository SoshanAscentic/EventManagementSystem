// <copyright file="UserEmailUpdatedEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.User
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.ValueObjects;

    public sealed class UserEmailUpdatedEvent : IDomainEvent
    {
        public UserEmailUpdatedEvent(UserId userId, Email oldEmail, Email newEmail)
        {
            this.UserId = userId;
            this.OldEmail = oldEmail;
            this.NewEmail = newEmail;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public UserId UserId { get; }

        public Email OldEmail { get; }

        public Email NewEmail { get; }
    }
}
