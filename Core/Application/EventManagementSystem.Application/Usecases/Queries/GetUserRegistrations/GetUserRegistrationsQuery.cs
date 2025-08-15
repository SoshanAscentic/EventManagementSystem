// <copyright file="GetUserRegistrationsQuery.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetUserRegistrations
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;

    public class GetUserRegistrationsQuery : IRequest<Result<PagedResult<RegistrationDto>>>
    {
        public GetUserRegistrationsQuery(int userId)
        {
            this.UserId = userId;
        }

        public int UserId { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
