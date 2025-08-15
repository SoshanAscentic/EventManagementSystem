// <copyright file="GetEventsNearingCapacityQuery.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEventsNearingCapacity
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;

    public class GetEventsNearingCapacityQuery : IRequest<Result<List<EventDto>>>
    {
        public double ThresholdPercentage { get; set; } = 0.8;
    }
}
