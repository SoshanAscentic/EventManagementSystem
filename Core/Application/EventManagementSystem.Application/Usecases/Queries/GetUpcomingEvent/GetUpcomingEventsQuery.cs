// <copyright file="GetUpcomingEventsQuery.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetUpcomingEvent
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;

    public class GetUpcomingEventsQuery : IRequest<Result<List<EventDto>>>
    {
        public int? CategoryId { get; set; }

        public int Count { get; set; } = 10;
    }
}
