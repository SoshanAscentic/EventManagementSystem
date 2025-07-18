// <copyright file="RegisterForEventCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.RegisterForEvent
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class RegisterForEventCommandHandler : IRequestHandler<RegisterForEventCommand, Result<int>>
    {
        private readonly IEventRepository eventRepository;
        private readonly IUserRepository userRepository;
        private readonly IEventRegistrationRepository registrationRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<RegisterForEventCommandHandler> logger;

        public RegisterForEventCommandHandler(
            IEventRepository eventRepository,
            IUserRepository userRepository,
            IEventRegistrationRepository registrationRepository,
            IUnitOfWork unitOfWork,
            ILogger<RegisterForEventCommandHandler> logger)
        {
            this.eventRepository = eventRepository;
            this.userRepository = userRepository;
            this.registrationRepository = registrationRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result<int>> Handle(RegisterForEventCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Registering user {UserId} for event {EventId}", request.UserId, request.EventId);

                var eventEntity = await this.eventRepository.GetByIdAsync(Domain.ValueObjects.EventId.Create(request.EventId), cancellationToken);
                if (eventEntity == null)
                {
                    this.logger.LogWarning("Event not found: {EventId}", request.EventId);
                    return DomainErrors.Event.NotFound(request.EventId);
                }

                var user = await this.userRepository.GetByIdAsync(UserId.Create(request.UserId), cancellationToken);
                if (user == null)
                {
                    this.logger.LogWarning("User not found: {UserId}", request.UserId);
                    return DomainErrors.User.NotFound(request.UserId);
                }

                // Check if user is already registered
                if (await this.registrationRepository.IsUserRegisteredForEventAsync(user.UserId, eventEntity.EventId, cancellationToken))
                {
                    this.logger.LogWarning("User already registered for event: {UserId}, {EventId}", request.UserId, request.EventId);
                    return DomainErrors.Registration.UserAlreadyRegistered(request.UserId, request.EventId);
                }

                // Check if registration is open
                if (!eventEntity.IsRegistrationOpen)
                {
                    this.logger.LogWarning("Registration closed for event: {EventId}", request.EventId);
                    return DomainErrors.Event.RegistrationClosed(request.EventId);
                }

                // Check if event is full
                if (eventEntity.IsFull)
                {
                    this.logger.LogWarning("Event is at full capacity: {EventId}", request.EventId);
                    return DomainErrors.Event.CapacityExceeded(request.EventId, eventEntity.CurrentRegistrations, eventEntity.Capacity.Value);
                }

                var registration = eventEntity.RegisterUser(user.UserId);
                if (!string.IsNullOrWhiteSpace(request.Notes))
                {
                    registration.AddNotes(request.Notes);
                }

                await this.registrationRepository.AddAsync(registration, cancellationToken);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation(
                    "Successfully registered user {UserId} for event {EventId} with registration ID: {RegistrationId}",
                    request.UserId,
                    request.EventId,
                    registration.Id);
                return registration.Id;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
            {
                this.logger.LogWarning(ex, "User already registered for event: {UserId}, {EventId}", request.UserId, request.EventId);
                return DomainErrors.Registration.UserAlreadyRegistered(request.UserId, request.EventId);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not open"))
            {
                this.logger.LogWarning(ex, "Registration not open for event: {EventId}", request.EventId);
                return DomainErrors.Event.RegistrationClosed(request.EventId);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("full capacity"))
            {
                this.logger.LogWarning(ex, "Event at full capacity: {EventId}", request.EventId);
                return DomainErrors.Event.CapacityExceeded(request.EventId, 0, 0);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID"))
            {
                this.logger.LogWarning(ex, "Invalid ID provided");
                return DomainErrors.General.InvalidId("User or Event");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error registering user {UserId} for event {EventId}", request.UserId, request.EventId);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
