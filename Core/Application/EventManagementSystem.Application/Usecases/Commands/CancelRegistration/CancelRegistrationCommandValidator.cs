// <copyright file="CancelRegistrationCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CancelRegistration
{
    using FluentValidation;

    public class CancelRegistrationCommandValidator : AbstractValidator<CancelRegistrationCommand>
    {
        public CancelRegistrationCommandValidator()
        {
            this.RuleFor(x => x.RegistrationId)
                .GreaterThan(0)
                .WithMessage("Registration ID must be a positive integer");

            this.RuleFor(x => x.Reason)
                .MaximumLength(500)
                .WithMessage("Cancellation reason cannot exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Reason));
        }
    }
}
