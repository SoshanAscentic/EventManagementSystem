// <copyright file="GetEventsQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEvents
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Application.Usecases.Queries.GetEvent;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetEventsQueryHandler : IRequestHandler<GetEventQuery, Result<EventDto>>
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

                var eventDto = this.mapper.Map<EventDto>(eventEntity);
                this.logger.LogInformation("Successfully retrieved event: {EventId}", request.Id);
                return eventDto;
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID"))
            {
                this.logger.LogWarning(ex, "Invalid event ID provided: {EventId}", request.Id);
                return DomainErrors.General.InvalidId("Event");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error getting event: {EventId}", request.Id);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
