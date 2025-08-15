// <copyright file="ApplicationService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Services
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Repositories;

    public abstract class ApplicationService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly ICurrentUserService currentUserService;

        protected ApplicationService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            this.unitOfWork = unitOfWork;
            this.currentUserService = currentUserService;
        }

        protected bool IsCurrentUserAdmin => this.currentUserService.IsAdmin;

        protected bool IsAuthenticated => this.currentUserService.IsAuthenticated;
    }
}
