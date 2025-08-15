// <copyright file="CategoryExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Extensions
{
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Entities;

    public static class CategoryExtensions
    {
        public static CategoryDto ToDto(this EventCategory entity)
        {
            return new CategoryDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                IsActive = entity.IsActive,
                EventCount = entity.Events.Count,
            };
        }

        public static List<CategoryDto> ToDto(this IEnumerable<EventCategory> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
