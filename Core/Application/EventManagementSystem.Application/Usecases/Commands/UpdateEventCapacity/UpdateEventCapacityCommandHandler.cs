// <copyright file="UpdateEventCapacityCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UpdateEventCapacity
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using FluentValidation;
    using MediatR;

    public class UpdateEventCapacityCommandHandler : IRequestHandler<UpdateEventCapacityCommand, Result>
    {
        private readonly IEventRepository eventRepository;
        private readonly IUnitOfWork unitOfWork;

        public UpdateEventCapacityCommandHandler(IEventRepository eventRepository, IUnitOfWork unitOfWork)
        {
            this.eventRepository = eventRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateEventCapacityCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await this.eventRepository.GetByIdWithRegistrationsAsync(
                EventId.Create(request.EventId),
                cancellationToken);

            if (eventEntity == null)
            {
                return Result.Failure("Event not found");
            }

            try
            {
                eventEntity.UpdateCapacity(request.NewCapacity);

                this.eventRepository.Update(eventEntity);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to update event capacity: {ex.Message}");
            }
        }
    }
}
