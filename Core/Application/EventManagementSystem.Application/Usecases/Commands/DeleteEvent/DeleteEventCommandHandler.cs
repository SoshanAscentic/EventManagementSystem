// <copyright file="DeleteEventCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.DeleteEvent
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Result>
    {
        private readonly IEventRepository eventRepository;
        private readonly IUnitOfWork unitOfWork;

        public DeleteEventCommandHandler(IEventRepository eventRepository, IUnitOfWork unitOfWork)
        {
            this.eventRepository = eventRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await eventRepository.GetByIdWithRegistrationsAsync(
                EventId.Create(request.Id),
                cancellationToken);

            if (eventEntity == null)
            {
                return Result.Failure("Event not found");
            }

            // Check if event has active registrations
            if (eventEntity.CurrentRegistrations > 0)
            {
                return Result.Failure("Cannot delete event with active registrations");
            }

            try
            {
                eventRepository.Remove(eventEntity);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to delete event: {ex.Message}");
            }
        }
    }
}
