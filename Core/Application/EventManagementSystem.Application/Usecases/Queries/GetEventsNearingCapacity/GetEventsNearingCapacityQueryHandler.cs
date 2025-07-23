// <copyright file="GetEventsNearingCapacityQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEventsNearingCapacity
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;

    public class GetEventsNearingCapacityQueryHandler : IRequestHandler<GetEventsNearingCapacityQuery, Result<List<EventDto>>>
    {
        private readonly IStatisticsService? statisticsService;
        private readonly IMapper? mapper;

        public async Task<Result<List<EventDto>>> Handle(GetEventsNearingCapacityQuery request, CancellationToken cancellationToken)
        {
            var events = await this.statisticsService.GetEventsNearingCapacityAsync(request.ThresholdPercentage, cancellationToken);
            var eventDtos = this.mapper.Map<List<EventDto>>(events);
            return Result<List<EventDto>>.Success(eventDtos);
        }
    }
}
