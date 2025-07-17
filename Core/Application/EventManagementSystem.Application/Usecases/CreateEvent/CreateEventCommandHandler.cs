// <copyright file="CreateEventCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.CreateEvent
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Result<int>>
    {
        private readonly IEventRepository eventRepository;
        private readonly IEventCategoryRepository categoryRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public CreateEventCommandHandler(
            IEventRepository eventRepository,
            IEventCategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            this.eventRepository = eventRepository;
            this.categoryRepository = categoryRepository;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<Result<int>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            // Validate category exists
            if (!await this.categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
            {
                return Result.Failure<int>("Category not found");
            }

            // Check for duplicate event
            if (await this.eventRepository.ExistsByTitleAndDateAsync(request.Title, request.StartDateTime, cancellationToken))
            {
                return Result.Failure<int>("An event with the same title and date already exists");
            }

            try
            {
                var eventEntity = Event.Create(
                    request.Title,
                    request.Description,
                    request.StartDateTime,
                    request.EndDateTime,
                    request.Venue,
                    request.Address,
                    request.Capacity,
                    EventType.Create(request.EventType),
                    request.CategoryId,
                    request.City,
                    request.Country);

                await this.eventRepository.AddAsync(eventEntity, cancellationToken);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success(eventEntity.Id);
            }
            catch (Exception ex)
            {
                return Result.Failure<int>($"Failed to create event: {ex.Message}");
            }
        }
    }
}
