// <copyright file="CancelRegistrationCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CancelRegistration
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class CancelRegistrationCommandHandler : IRequestHandler<CancelRegistrationCommand, Result>
    {
        private readonly IEventRegistrationRepository registrationRepository;
        private readonly IUnitOfWork unitOfWork;

        public CancelRegistrationCommandHandler(
            IEventRegistrationRepository registrationRepository,
            IUnitOfWork unitOfWork)
        {
            this.registrationRepository = registrationRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(CancelRegistrationCommand request, CancellationToken cancellationToken)
        {
            var registration = await this.registrationRepository.GetByIdAsync(
                RegistrationId.Create(request.RegistrationId),
                cancellationToken);

            if (registration == null)
            {
                return Result.Failure("Registration not found");
            }

            try
            {
                registration.Cancel(request.Reason);
                this.registrationRepository.Update(registration);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to cancel registration: {ex.Message}");
            }
        }
    }
}
