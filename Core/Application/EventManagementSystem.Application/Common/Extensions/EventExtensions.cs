// <copyright file="EventExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Extensions
{
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Entities;

    public static class EventExtensions
    {
        public static EventDto ToDto(this Event entity)
        {
            return new EventDto
            {
                Id = entity.Id,
                Title = entity.Title,
                Description = entity.Description,
                StartDateTime = entity.EventDateTime.StartDateTime,
                EndDateTime = entity.EventDateTime.EndDateTime,
                Venue = entity.Location.Venue,
                Address = entity.Location.Address,
                City = entity.Location.City,
                Country = entity.Location.Country,
                Capacity = entity.Capacity.Value,
                EventType = entity.EventType.Value,
                CategoryId = entity.CategoryId,
                CategoryName = entity.Category?.Name ?? "Unknown",
                CurrentRegistrations = entity.CurrentRegistrations,
                RemainingCapacity = entity.RemainingCapacity,
                IsRegistrationOpen = entity.IsRegistrationOpen,
                PrimaryImageUrl = entity.PrimaryImage?.FilePath,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
            };
        }

        public static List<EventDto> ToDto(this IEnumerable<Event> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
