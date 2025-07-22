// <copyright file="GetEventStatisticsQuery.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEventStatistics
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;

    public class GetEventStatisticsQuery : IRequest<Result<EventStatisticsDto>>
    {
        public DateTime? FromDate { get; set; }
    }
}
