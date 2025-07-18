// <copyright file="MarkAttendanceCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.MarkAttendance
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, Result>
    {
        private readonly IEventRegistrationRepository registrationRepository;
        private readonly IUnitOfWork unitOfWork;

        public MarkAttendanceCommandHandler(
            IEventRegistrationRepository registrationRepository,
            IUnitOfWork unitOfWork)
        {
            this.registrationRepository = registrationRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
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

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to mark attendance: {ex.Message}");
            }
        }
    }
}
