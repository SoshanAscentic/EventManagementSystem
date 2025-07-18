// <copyright file="GetEventQuery.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEvent
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;

    public class GetEventQuery : IRequest<Result<EventDto>>
    {
        public GetEventQuery(int id)
        {
            this.Id = id;
        }

        public int Id { get; set; }
    }
}
