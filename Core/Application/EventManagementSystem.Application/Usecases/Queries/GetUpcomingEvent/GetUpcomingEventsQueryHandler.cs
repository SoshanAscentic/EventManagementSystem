// <copyright file="GetUpcomingEventsQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetUpcomingEvent
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using MediatR;

    public class GetUpcomingEventsQueryHandler : IRequestHandler<GetUpcomingEventsQuery, Result<List<EventDto>>>
    {
        private readonly IEventRepository eventRepository;
        private readonly IMapper mapper;

        public GetUpcomingEventsQueryHandler(IEventRepository eventRepository, IMapper mapper)
        {
            this.eventRepository = eventRepository;
            this.mapper = mapper;
        }

        public async Task<Result<List<EventDto>>> Handle(GetUpcomingEventsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var upcomingEvents = await this.eventRepository.GetUpcomingEventsAsync(
                    request.CategoryId,
                    cancellationToken);

                var limitedEvents = upcomingEvents.Take(request.Count).ToList();
                var eventDtos = this.mapper.Map<List<EventDto>>(limitedEvents);

                return Result.Success(eventDtos);
            }
            catch (Exception ex)
            {
                return Result.Failure<List<EventDto>>($"Failed to retrieve upcoming events: {ex.Message}");
            }
        }
    }
}
