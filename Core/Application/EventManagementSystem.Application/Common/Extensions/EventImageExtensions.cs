// <copyright file="EventImageExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Extensions
{
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Entities;

    public static class EventImageExtensions
    {
        public static EventImageDto ToDto(this EventImage entity)
        {
            return new EventImageDto
            {
                Id = entity.Id,
                EventId = entity.EventId.Value,
                FileName = entity.FileName,
                FilePath = entity.FilePath,
                FileSize = entity.FileSize,
                IsPrimary = entity.IsPrimary,
                UploadedAt = entity.UploadedAt,
            };
        }

        public static List<EventImageDto> ToDto(this IEnumerable<EventImage> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
