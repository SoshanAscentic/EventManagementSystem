// <copyright file="MarkAttendanceCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.MarkAttendance
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, Result>
    {
        private readonly IEventRegistrationRepository registrationRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<MarkAttendanceCommandHandler> logger;

        public MarkAttendanceCommandHandler(
            IEventRegistrationRepository registrationRepository,
            IUnitOfWork unitOfWork,
            ILogger<MarkAttendanceCommandHandler> logger)
        {
            this.registrationRepository = registrationRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation(
                    "Marking attendance for registration: {RegistrationId}, Attended: {Attended}",
                    request.RegistrationId,
                    request.Attended);

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
                    this.logger.LogWarning("Cannot mark attendance for cancelled registration: {RegistrationId}", request.RegistrationId);
                    return DomainErrors.Registration.CannotMarkAttendedForCancelled(request.RegistrationId);
                }

                if (request.Attended)
                {
                    registration.MarkAsAttended();
                }
                else
                {
                    registration.MarkAsNoShow();
                }

                this.registrationRepository.Update(registration);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation("Successfully marked attendance for registration: {RegistrationId}", request.RegistrationId);
                return Result.Success();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("cancelled"))
            {
                this.logger.LogWarning(ex, "Cannot mark attendance for cancelled registration: {RegistrationId}", request.RegistrationId);
                return DomainErrors.Registration.CannotMarkAttendedForCancelled(request.RegistrationId);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("already"))
            {
                this.logger.LogWarning(ex, "Registration already attended: {RegistrationId}", request.RegistrationId);
                return DomainErrors.Registration.AlreadyAttended(request.RegistrationId);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID"))
            {
                this.logger.LogWarning(ex, "Invalid registration ID provided: {RegistrationId}", request.RegistrationId);
                return DomainErrors.General.InvalidId("Registration");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error marking attendance for registration: {RegistrationId}", request.RegistrationId);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
