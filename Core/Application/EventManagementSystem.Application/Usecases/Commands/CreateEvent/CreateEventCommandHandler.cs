// <copyright file="CreateEventCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CreateEvent
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Result<EventDto>>
    {
        private readonly IEventRepository eventRepository;
        private readonly IEventCategoryRepository categoryRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger<CreateEventCommandHandler> logger;

        public CreateEventCommandHandler(
            IEventRepository eventRepository,
            IEventCategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateEventCommandHandler> logger)
        {
            this.eventRepository = eventRepository;
            this.categoryRepository = categoryRepository;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<EventDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Creating event: {Title}", request.Title);

                // Validate category exists
                if (!await this.categoryRepository.ExistsAsync(request.CategoryId, cancellationToken))
                {
                    this.logger.LogWarning("Category not found: {CategoryId}", request.CategoryId);
                    return DomainErrors.Event.CategoryNotFound(request.CategoryId);
                }

                // Check for duplicate title and date
                if (await this.eventRepository.ExistsByTitleAndDateAsync(request.Title, request.StartDateTime, cancellationToken))
                {
                    this.logger.LogWarning(
                        "Duplicate event creation attempted: {Title} on {Date}",
                        request.Title,
                        request.StartDateTime);
                    return DomainErrors.Event.DuplicateTitle(request.Title, request.StartDateTime);
                }

                // Create event using domain factory method
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

                // Add to repository
                await this.eventRepository.AddAsync(eventEntity, cancellationToken);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                // Map to DTO and return success
                var eventDto = this.mapper.Map<EventDto>(eventEntity);
                this.logger.LogInformation("Successfully created event with ID: {EventId}", eventEntity.Id);

                return Result<EventDto>.Success(eventDto);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("title"))
            {
                this.logger.LogWarning(ex, "Invalid title provided: {Title}", request.Title);
                return ex.Message.Contains("empty")
                    ? DomainErrors.Event.TitleEmpty()
                    : DomainErrors.Event.TitleTooLong(200);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("description"))
            {
                this.logger.LogWarning(ex, "Invalid description provided");
                return ex.Message.Contains("empty")
                    ? DomainErrors.Event.DescriptionEmpty()
                    : DomainErrors.Event.DescriptionTooLong(2000);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("capacity"))
            {
                this.logger.LogWarning(ex, "Invalid capacity provided: {Capacity}", request.Capacity);
                return DomainErrors.Event.InvalidCapacity();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("past"))
            {
                this.logger.LogWarning(ex, "Past start date provided: {StartDate}", request.StartDateTime);
                return DomainErrors.Event.PastStartDate();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error creating event: {Title}", request.Title);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
