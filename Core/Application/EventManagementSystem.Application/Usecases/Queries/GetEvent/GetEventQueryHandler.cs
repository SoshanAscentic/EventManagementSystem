// <copyright file="GetEventQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEvents
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Extensions;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Application.Usecases.Queries.GetEvent;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetEventQueryHandler : IRequestHandler<GetEventQuery, Result<EventDto>>
    {
        private readonly IEventRepository eventRepository;
        private readonly ILogger<GetEventQueryHandler> logger;

        public GetEventQueryHandler(
            IEventRepository eventRepository,
            ILogger<GetEventQueryHandler> logger)
        {
            this.eventRepository = eventRepository;
            this.logger = logger;
        }

        public async Task<Result<EventDto>> Handle(GetEventQuery request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Getting event: {EventId}", request.Id);

                var eventEntity = await this.eventRepository.GetByIdWithAllDetailsAsync(
                    Domain.ValueObjects.EventId.Create(request.Id),
                    cancellationToken);

                if (eventEntity == null)
                {
                    this.logger.LogWarning("Event not found: {EventId}", request.Id);
                    return DomainErrors.Event.NotFound(request.Id);
                }

                var eventDto = eventEntity.ToDto();
                this.logger.LogInformation("Successfully retrieved event: {EventId}", request.Id);
                return eventDto;
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID"))
            {
                this.logger.LogWarning(ex, "Invalid event ID provided: {EventId}", request.Id);
                return Result<EventDto>.ValidationFailure("General.InvalidId", $"Invalid event ID: {request.Id}");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error getting event: {EventId}", request.Id);
                return Result<EventDto>.Failure("General.UnexpectedError", "An unexpected error occurred while retrieving the event");
            }
        }
    }
}
