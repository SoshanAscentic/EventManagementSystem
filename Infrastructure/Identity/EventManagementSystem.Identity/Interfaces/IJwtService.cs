// <copyright file="IJwtService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Identity.Interfaces
{
    using System.Security.Claims;
    using EventManagementSystem.Identity.Entities;

    public interface IJwtService
    {
        string GenerateAccessToken(ApplicationUser user, IList<string> roles);

        RefreshToken GenerateRefreshToken(string ipAddress);

        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

        Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user);

        Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    }
}
