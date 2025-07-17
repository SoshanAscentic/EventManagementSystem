// <copyright file="GetEventRegistrationsQuery.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEventRegistration
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;

    public class GetEventRegistrationsQuery : IRequest<Result<PagedResult<RegistrationDto>>>
    {
        public GetEventRegistrationsQuery(int eventId)
        {
            this.EventId = eventId;
        }

        public int EventId { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 50;

        public string? Status { get; set; }
    }
}
