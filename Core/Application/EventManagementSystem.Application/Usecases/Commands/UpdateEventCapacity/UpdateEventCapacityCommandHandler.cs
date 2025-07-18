// <copyright file="UpdateEventCapacityCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UpdateEventCapacity
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using FluentValidation;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class UpdateEventCapacityCommandHandler : IRequestHandler<UpdateEventCapacityCommand, Result>
    {
        private readonly IEventRepository eventRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<UpdateEventCapacityCommandHandler> logger;

        public UpdateEventCapacityCommandHandler(
            IEventRepository eventRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateEventCapacityCommandHandler> logger)
        {
            this.eventRepository = eventRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result> Handle(UpdateEventCapacityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Updating event capacity: {EventId} to {NewCapacity}", request.EventId, request.NewCapacity);

                var eventEntity = await this.eventRepository.GetByIdWithRegistrationsAsync(
                    Domain.ValueObjects.EventId.Create(request.EventId),
                    cancellationToken);

                if (eventEntity == null)
                {
                    this.logger.LogWarning("Event not found: {EventId}", request.EventId);
                    return DomainErrors.Event.NotFound(request.EventId);
                }

                // Check if new capacity is less than current registrations
                if (request.NewCapacity < eventEntity.CurrentRegistrations)
                {
                    this.logger.LogWarning(
                        "Cannot reduce capacity below current registrations: {EventId}, Current: {Current}, New: {New}",
                        request.EventId,
                        eventEntity.CurrentRegistrations,
                        request.NewCapacity);
                    return DomainErrors.Event.CapacityExceeded(request.EventId, eventEntity.CurrentRegistrations, request.NewCapacity);
                }

                eventEntity.UpdateCapacity(request.NewCapacity);

                this.eventRepository.Update(eventEntity);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation("Successfully updated event capacity: {EventId} to {NewCapacity}", request.EventId, request.NewCapacity);
                return Result.Success();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("below current registrations"))
            {
                this.logger.LogWarning(ex, "Cannot reduce capacity below current registrations: {EventId}", request.EventId);
                return DomainErrors.Event.CapacityExceeded(request.EventId, 0, request.NewCapacity);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("capacity"))
            {
                this.logger.LogWarning(ex, "Invalid capacity provided: {Capacity}", request.NewCapacity);
                return DomainErrors.Event.InvalidCapacity();
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID"))
            {
                this.logger.LogWarning(ex, "Invalid event ID provided: {EventId}", request.EventId);
                return DomainErrors.General.InvalidId("Event");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error updating event capacity: {EventId}", request.EventId);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
