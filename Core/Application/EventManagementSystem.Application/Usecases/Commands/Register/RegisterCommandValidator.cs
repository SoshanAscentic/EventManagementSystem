// <copyright file="RegisterCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.Register
{
    using FluentValidation;

    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            this.RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            this.RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6);

            this.RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password);

            this.RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(50);

            this.RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(50);
        }
    }
}
