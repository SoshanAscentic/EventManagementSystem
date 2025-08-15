// <copyright file="DeleteEventCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.DeleteEvent
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Result>
    {
        private readonly IEventRepository eventRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<DeleteEventCommandHandler> logger;

        public DeleteEventCommandHandler(
            IEventRepository eventRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeleteEventCommandHandler> logger)
        {
            this.eventRepository = eventRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Deleting event: {EventId}", request.Id);

                var eventEntity = await this.eventRepository.GetByIdWithRegistrationsAsync(
                    Domain.ValueObjects.EventId.Create(request.Id),
                    cancellationToken);

                if (eventEntity == null)
                {
                    this.logger.LogWarning("Event not found: {EventId}", request.Id);
                    return DomainErrors.Event.NotFound(request.Id);
                }

                // Check if event has active registrations
                if (eventEntity.CurrentRegistrations > 0)
                {
                    this.logger.LogWarning(
                        "Cannot delete event with active registrations: {EventId}, Registrations: {Count}",
                        request.Id,
                        eventEntity.CurrentRegistrations);
                    return DomainErrors.Event.HasActiveRegistrations(request.Id, eventEntity.CurrentRegistrations);
                }

                // Check if event has already started
                if (eventEntity.IsOngoing || eventEntity.IsCompleted)
                {
                    this.logger.LogWarning("Cannot delete event that has started: {EventId}", request.Id);
                    return DomainErrors.Event.AlreadyStarted(request.Id);
                }

                this.eventRepository.Remove(eventEntity);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation("Successfully deleted event: {EventId}", request.Id);
                return Result.Success();
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID"))
            {
                this.logger.LogWarning(ex, "Invalid event ID provided: {EventId}", request.Id);
                return DomainErrors.General.InvalidId("Event");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error deleting event: {EventId}", request.Id);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
