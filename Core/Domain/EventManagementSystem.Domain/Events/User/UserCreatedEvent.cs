// <copyright file="UserCreatedEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.User
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.ValueObjects;

    public sealed class UserCreatedEvent : IDomainEvent
    {
        public UserCreatedEvent(UserId userId, Email email, string fullName)
        {
            this.UserId = userId;
            this.Email = email;
            this.FullName = fullName;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public UserId UserId { get; }

        public Email Email { get; }

        public string FullName { get; }
    }
}
