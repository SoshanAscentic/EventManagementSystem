// <copyright file="CurrentUserService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Identity.Services
{
    using System.Security.Claims;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.ValueObjects;
    using Microsoft.AspNetCore.Http;

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public UserId? UserId
        {
            get
            {
                var userIdClaim = this.httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    return Domain.ValueObjects.UserId.Create(userId);
                }

                return null;
            }
        }

        public string? Email => this.httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

        public bool IsAuthenticated => this.httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

        public bool IsAdmin => this.httpContextAccessor.HttpContext?.User?.IsInRole("Admin") == true;

        public async Task<bool> IsInRoleAsync(string role)
        {
            return await Task.FromResult(this.httpContextAccessor.HttpContext?.User?.IsInRole(role) == true);
        }
    }
}
