// <copyright file="GetEventsQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEvents
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, Result<PagedResult<EventDto>>>
    {
        private readonly IEventRepository eventRepository;
        private readonly IMapper mapper;

        public GetEventsQueryHandler(IEventRepository eventRepository, IMapper mapper)
        {
            this.eventRepository = eventRepository;
            this.mapper = mapper;
        }

        public async Task<Result<PagedResult<EventDto>>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                EventType? eventType = null;
                if (!string.IsNullOrEmpty(request.EventType))
                {
                    eventType = EventType.Create(request.EventType);
                }

                var (events, totalCount) = await this.eventRepository.SearchEventsAsync(
                    request.SearchTerm,
                    request.CategoryId,
                    eventType,
                    request.StartDate,
                    request.EndDate,
                    request.Location,
                    request.HasAvailableSpots,
                    request.PageNumber,
                    request.PageSize,
                    request.SortBy,
                    request.Ascending,
                    cancellationToken);

                var eventDtos = this.mapper.Map<List<EventDto>>(events);
                var pagedResult = new PagedResult<EventDto>(eventDtos, totalCount, request.PageNumber, request.PageSize);

                return Result.Success(pagedResult);
            }
            catch (Exception ex)
            {
                return Result.Failure<PagedResult<EventDto>>($"Failed to retrieve events: {ex.Message}");
            }
        }
    }
}
