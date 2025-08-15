// <copyright file="BaseEntity.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Common
{
    using System;
    using System.Collections.Generic;

    public class BaseEntity : IAggregateRoot
    {
        private readonly List<IDomainEvent> domainEvents = new ();

        public int Id { get; protected set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public IReadOnlyCollection<IDomainEvent> DomainEvents => this.domainEvents.AsReadOnly();

        public void ClearDomainEvents()
        {
            this.domainEvents.Clear();
        }

        public void MarkAsUpdated()
        {
            this.UpdatedAt = DateTime.UtcNow;
        }

        public void SetCreatedAt(DateTime createdAt)
        {
            this.CreatedAt = createdAt;
        }

        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);
            this.domainEvents.Add(domainEvent);
        }

        protected void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);
            this.domainEvents.Remove(domainEvent);
        }
    }
}
