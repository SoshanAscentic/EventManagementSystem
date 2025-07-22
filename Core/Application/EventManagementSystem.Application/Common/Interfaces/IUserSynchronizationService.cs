// <copyright file="IUserSynchronizationService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Interfaces
{
    using EventManagementSystem.Application.Common.Models;

    public interface IUserSynchronizationService
    {
        Task<Result> SynchronizeUserAsync(int identityUserId, CancellationToken cancellationToken = default);

        Task<Result> UpdateDomainUserFromIdentityAsync(int identityUserId, CancellationToken cancellationToken = default);

        Task<Result> HandleIdentityUserUpdatedAsync(int identityUserId, CancellationToken cancellationToken = default);

        Task<Result> HandleIdentityUserDeletedAsync(int identityUserId, CancellationToken cancellationToken = default);
    }
}
