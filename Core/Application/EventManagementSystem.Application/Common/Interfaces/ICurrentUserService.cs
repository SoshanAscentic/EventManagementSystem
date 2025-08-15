// <copyright file="ICurrentUserService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Interfaces
{
    using System.Threading.Tasks;
    using EventManagementSystem.Domain.ValueObjects;

    public interface ICurrentUserService
    {
        UserId? UserId { get; }

        string? Email { get; }

        bool IsAuthenticated { get; }

        bool IsAdmin { get; }

        Task<bool> IsInRoleAsync(string role);
    }
}
