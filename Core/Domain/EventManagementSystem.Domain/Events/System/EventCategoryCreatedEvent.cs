// <copyright file="EventCategoryCreatedEvent.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Events.System
{
    using EventManagementSystem.Domain.Common;

    public sealed class EventCategoryCreatedEvent : IDomainEvent
    {
        public EventCategoryCreatedEvent(int categoryId, string categoryName, string description)
        {
            this.CategoryId = categoryId;
            this.CategoryName = categoryName;
            this.Description = description;
        }

        public Guid EventId { get; } = Guid.NewGuid();

        public DateTime OccurredOn { get; } = DateTime.UtcNow;

        public int CategoryId { get; }

        public string CategoryName { get; }

        public string Description { get; }
    }
}
