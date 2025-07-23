// <copyright file="UserExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Extensions
{
    using System;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Entities;

    public static class UserExtensions
    {
        public static UserDto ToDto(this User entity)
        {
            return new UserDto
            {
                Id = entity.Id,
                Email = entity.Email.Value,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                FullName = entity.FullName,
                Phone = entity.Phone?.Value,
                ActiveRegistrationsCount = entity.ActiveRegistrationsCount,
                CreatedAt = entity.CreatedAt,
            };
        }

        public static List<UserDto> ToDto(this IEnumerable<User> entities)
        {
            return entities.Select(e => e.ToDto()).ToList();
        }
    }
}
