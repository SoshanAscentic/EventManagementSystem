// <copyright file="LoginCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.Login
{
    using FluentValidation;

    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            this.RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            this.RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6);
        }
    }

}
