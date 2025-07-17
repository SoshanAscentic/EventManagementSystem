// <copyright file="RegisterForEventCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.RegisterForEvent
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class RegisterForEventCommandHandler : IRequestHandler<RegisterForEventCommand, Result<int>>
    {
        private readonly IEventRepository eventRepository;
        private readonly IUserRepository userRepository;
        private readonly IEventRegistrationRepository registrationRepository;
        private readonly IUnitOfWork unitOfWork;

        public RegisterForEventCommandHandler(
            IEventRepository eventRepository,
            IUserRepository userRepository,
            IEventRegistrationRepository registrationRepository,
            IUnitOfWork unitOfWork)
        {
            this.eventRepository = eventRepository;
            this.userRepository = userRepository;
            this.registrationRepository = registrationRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(RegisterForEventCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await eventRepository.GetByIdAsync(EventId.Create(request.EventId), cancellationToken);
            if (eventEntity == null)
            {
                return Result.Failure<int>("Event not found");
            }

            var user = await userRepository.GetByIdAsync(UserId.Create(request.UserId), cancellationToken);
            if (user == null)
            {
                return Result.Failure<int>("User not found");
            }

            // Check if user can register
            if (!user.CanRegisterForEvent(eventEntity))
            {
                return Result.Failure<int>("User cannot register for this event");
            }

            try
            {
                var registration = eventEntity.RegisterUser(user.UserId);
                if (!string.IsNullOrWhiteSpace(request.Notes))
                {
                    registration.AddNotes(request.Notes);
                }

                await registrationRepository.AddAsync(registration, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success(registration.Id);
            }
            catch (Exception ex)
            {
                return Result.Failure<int>($"Failed to register for event: {ex.Message}");
            }
        }
    }
}
