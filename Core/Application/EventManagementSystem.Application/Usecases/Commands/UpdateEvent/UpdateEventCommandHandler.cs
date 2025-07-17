// <copyright file="UpdateEventCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UpdateEvent
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Result>
    {
        private readonly IEventRepository eventRepository;
        private readonly IUnitOfWork unitOfWork;

        public UpdateEventCommandHandler(IEventRepository eventRepository, IUnitOfWork unitOfWork)
        {
            this.eventRepository = eventRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await this.eventRepository.GetByIdAsync(EventId.Create(request.Id), cancellationToken);
            if (eventEntity == null)
            {
                return Result.Failure("Event not found");
            }

            try
            {
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

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to update event: {ex.Message}");
            }
        }
    }
}
