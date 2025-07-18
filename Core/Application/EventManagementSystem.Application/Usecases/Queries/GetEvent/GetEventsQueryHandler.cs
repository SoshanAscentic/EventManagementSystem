// <copyright file="GetEventsQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEvent
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Application.Usecases.Queries.GetEvents;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, Result<PagedResult<EventDto>>>
    {
        private readonly IEventRepository eventRepository;
        private readonly IMapper mapper;
        private readonly ILogger<GetEventsQueryHandler> logger;

        public GetEventsQueryHandler(
            IEventRepository eventRepository,
            IMapper mapper,
            ILogger<GetEventsQueryHandler> logger)
        {
            this.eventRepository = eventRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<PagedResult<EventDto>>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation(
                    "Getting events with filters - SearchTerm: {SearchTerm}, CategoryId: {CategoryId}, Page: {Page}",
                    request.SearchTerm,
                    request.CategoryId,
                    request.PageNumber);

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

                this.logger.LogInformation("Successfully retrieved {Count} events out of {Total}", events.Count, totalCount);
                return pagedResult;
            }
            catch (ArgumentException ex) when (ex.Message.Contains("event type"))
            {
                this.logger.LogWarning(ex, "Invalid event type provided: {EventType}", request.EventType);
                return DomainErrors.Event.InvalidEventType(request.EventType ?? string.Empty);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("page"))
            {
                this.logger.LogWarning(ex, "Invalid pagination parameters");
                return DomainErrors.General.ValidationFailed("Invalid pagination parameters");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error getting events");
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
