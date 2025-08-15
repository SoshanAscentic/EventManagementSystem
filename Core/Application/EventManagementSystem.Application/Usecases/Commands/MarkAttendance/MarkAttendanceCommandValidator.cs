// <copyright file="MarkAttendanceCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.MarkAttendance
{
    using FluentValidation;

    public class MarkAttendanceCommandValidator : AbstractValidator<MarkAttendanceCommand>
    {
        public MarkAttendanceCommandValidator()
        {
            this.RuleFor(x => x.RegistrationId)
                .GreaterThan(0)
                .WithMessage("Registration ID must be a positive integer");

            this.RuleFor(x => x.Attended)
                .NotNull()
                .WithMessage("Attended status must be specified");
        }
    }
}
