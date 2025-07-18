// <copyright file="UpdateEventCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UpdateEvent
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Result>
    {
        private readonly IEventRepository eventRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<UpdateEventCommandHandler> logger;

        public UpdateEventCommandHandler(
            IEventRepository eventRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateEventCommandHandler> logger)
        {
            this.eventRepository = eventRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Updating event: {EventId}", request.Id);

                var eventEntity = await this.eventRepository.GetByIdAsync(Domain.ValueObjects.EventId.Create(request.Id), cancellationToken);
                if (eventEntity == null)
                {
                    this.logger.LogWarning("Event not found: {EventId}", request.Id);
                    return DomainErrors.Event.NotFound(request.Id);
                }

                // Check if event has already started or completed
                if (eventEntity.IsCompleted)
                {
                    this.logger.LogWarning("Cannot update completed event: {EventId}", request.Id);
                    return DomainErrors.Event.AlreadyStarted(request.Id);
                }

                if (eventEntity.IsOngoing)
                {
                    this.logger.LogWarning("Cannot update ongoing event: {EventId}", request.Id);
                    return DomainErrors.Event.AlreadyStarted(request.Id);
                }

                eventEntity.UpdateDetails(
                    request.Title,
                    request.Description,
                    request.StartDateTime,
                    request.EndDateTime,
                    request.Venue,
                    request.Address,
                    request.City,
                    request.Country);

                this.eventRepository.Update(eventEntity);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation("Successfully updated event: {EventId}", request.Id);
                return Result.Success();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("completed"))
            {
                this.logger.LogWarning(ex, "Cannot update completed event: {EventId}", request.Id);
                return DomainErrors.Event.AlreadyStarted(request.Id);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("ongoing"))
            {
                this.logger.LogWarning(ex, "Cannot update ongoing event: {EventId}", request.Id);
                return DomainErrors.Event.AlreadyStarted(request.Id);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("registrations exist"))
            {
                this.logger.LogWarning(ex, "Cannot change event date with existing registrations: {EventId}", request.Id);
                return DomainErrors.Event.HasActiveRegistrations(request.Id, 0);
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
            catch (ArgumentException ex) when (ex.Message.Contains("ID"))
            {
                this.logger.LogWarning(ex, "Invalid event ID provided: {EventId}", request.Id);
                return DomainErrors.General.InvalidId("Event");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error updating event: {EventId}", request.Id);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
