// <copyright file="RegistrationExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Extensions
{
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Entities;

    public static class RegistrationExtensions
    {
        public static RegistrationDto ToDto(this EventRegistration entity)
        {
            return new RegistrationDto
            {
                Id = entity.Id,
                EventId = entity.EventId.Value,
                UserId = entity.UserId.Value,
                EventTitle = entity.Event?.Title ?? "Unknown",
                UserName = entity.User?.FullName ?? "Unknown",
                UserEmail = entity.User?.Email.Value ?? "Unknown",
                RegisteredAt = entity.RegisteredAt,
                CancelledAt = entity.CancelledAt,
                Status = entity.Status.Value,
                Notes = entity.Notes,
                IsActive = entity.IsActive,
                IsCancelled = entity.IsCancelled,
            };
        }

        public static List<RegistrationDto> ToDto(this IEnumerable<EventRegistration> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
