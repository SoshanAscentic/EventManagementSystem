// <copyright file="GetEventsQuery.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEvents
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;

    public class GetEventsQuery : IRequest<Result<PagedResult<EventDto>>>
    {
        public string? SearchTerm { get; set; }

        public int? CategoryId { get; set; }

        public string? EventType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Location { get; set; }

        public bool? HasAvailableSpots { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public string SortBy { get; set; } = "StartDateTime";

        public bool Ascending { get; set; } = true;
    }
}
