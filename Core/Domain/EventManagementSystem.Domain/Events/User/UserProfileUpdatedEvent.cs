// <copyright file="UserProfileUpdatedEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.User
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.ValueObjects;

    public sealed class UserProfileUpdatedEvent : IDomainEvent
    {
        public UserProfileUpdatedEvent(UserId userId, Email email, string newFullName, string oldFullName)
        {
            this.UserId = userId;
            this.Email = email;
            this.NewFullName = newFullName;
            this.OldFullName = oldFullName;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public UserId UserId { get; }

        public Email Email { get; }

        public string NewFullName { get; }

        public string OldFullName { get; }
    }
}
