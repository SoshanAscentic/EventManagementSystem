// <copyright file="CancelRegistrationCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CancelRegistration
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class CancelRegistrationCommandHandler : IRequestHandler<CancelRegistrationCommand, Result>
    {
        private readonly IEventRegistrationRepository registrationRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<CancelRegistrationCommandHandler> logger;

        public CancelRegistrationCommandHandler(
            IEventRegistrationRepository registrationRepository,
            IUnitOfWork unitOfWork,
            ILogger<CancelRegistrationCommandHandler> logger)
        {
            this.registrationRepository = registrationRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result> Handle(CancelRegistrationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Cancelling registration: {RegistrationId}", request.RegistrationId);

                var registration = await this.registrationRepository.GetByIdAsync(
                    RegistrationId.Create(request.RegistrationId),
                    cancellationToken);

                if (registration == null)
                {
                    this.logger.LogWarning("Registration not found: {RegistrationId}", request.RegistrationId);
                    return DomainErrors.Registration.NotFound(request.RegistrationId);
                }

                if (registration.IsCancelled)
                {
                    this.logger.LogWarning("Registration already cancelled: {RegistrationId}", request.RegistrationId);
                    return DomainErrors.Registration.AlreadyCancelled(request.RegistrationId, registration.CancelledAt!.Value);
                }

                registration.Cancel(request.Reason);
                this.registrationRepository.Update(registration);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation("Successfully cancelled registration: {RegistrationId}", request.RegistrationId);
                return Result.Success();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already cancelled"))
            {
                this.logger.LogWarning(ex, "Registration already cancelled: {RegistrationId}", request.RegistrationId);
                return DomainErrors.Registration.AlreadyCancelled(request.RegistrationId, DateTime.UtcNow);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("24 hours"))
            {
                this.logger.LogWarning(ex, "Cannot cancel registration within deadline: {RegistrationId}", request.RegistrationId);
                return DomainErrors.Registration.CannotCancelPastDeadline(request.RegistrationId, DateTime.UtcNow.AddHours(24));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error cancelling registration: {RegistrationId}", request.RegistrationId);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
