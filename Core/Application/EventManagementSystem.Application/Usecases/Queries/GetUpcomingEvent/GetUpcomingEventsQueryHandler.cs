// <copyright file="GetUpcomingEventsQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetUpcomingEvent
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Extensions;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetUpcomingEventsQueryHandler : IRequestHandler<GetUpcomingEventsQuery, Result<List<EventDto>>>
    {
        private readonly IEventRepository eventRepository;
        private readonly ILogger<GetUpcomingEventsQueryHandler> logger;

        public GetUpcomingEventsQueryHandler(
            IEventRepository eventRepository,
            ILogger<GetUpcomingEventsQueryHandler> logger)
        {
            this.eventRepository = eventRepository;
            this.logger = logger;
        }

        public async Task<Result<List<EventDto>>> Handle(GetUpcomingEventsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Getting upcoming events, CategoryId: {CategoryId}, Count: {Count}", request.CategoryId, request.Count);

                var upcomingEvents = await this.eventRepository.GetUpcomingEventsAsync(
                    request.CategoryId,
                    cancellationToken);

                var limitedEvents = upcomingEvents.Take(request.Count).ToList();
                var eventDtos = limitedEvents.ToDto();

                this.logger.LogInformation("Successfully retrieved {Count} upcoming events", eventDtos.Count);
                return eventDtos;
            }
            catch (ArgumentException ex) when (ex.Message.Contains("category"))
            {
                this.logger.LogWarning(ex, "Invalid category ID provided: {CategoryId}", request.CategoryId);
                return Result<List<EventDto>>.ValidationFailure("Category.InvalidId", $"Invalid category ID: {request.CategoryId}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error getting upcoming events");
                return Result<List<EventDto>>.Failure("General.UnexpectedError", "An unexpected error occurred while retrieving upcoming events");
            }
        }
    }
}
