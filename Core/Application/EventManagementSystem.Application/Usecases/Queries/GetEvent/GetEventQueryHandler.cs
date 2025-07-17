// <copyright file="GetEventQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEvent
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class GetEventQueryHandler : IRequestHandler<GetEventQuery, Result<EventDto>>
    {
        private readonly IEventRepository eventRepository;
        private readonly IMapper mapper;

        public GetEventQueryHandler(IEventRepository eventRepository, IMapper mapper)
        {
            this.eventRepository = eventRepository;
            this.mapper = mapper;
        }

        public async Task<Result<EventDto>> Handle(GetEventQuery request, CancellationToken cancellationToken)
        {
            var eventEntity = await eventRepository.GetByIdWithAllDetailsAsync(
                EventId.Create(request.Id),
                cancellationToken);

            if (eventEntity == null)
            {
                return Result.Failure<EventDto>("Event not found");
            }

            var eventDto = mapper.Map<EventDto>(eventEntity);
            return Result.Success(eventDto);
        }
    }
}
