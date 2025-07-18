// <copyright file="CreateUserCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CreateUser
{
    using FluentValidation;

    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            this.RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(254);

            this.RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(50);

            this.RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(50);

            this.RuleFor(x => x.Phone)
                .Must(BeValidPhoneOrEmpty)
                .WithMessage("Invalid phone format")
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));
        }

        private static bool BeValidPhoneOrEmpty(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return true;
            }

            try
            {
                Domain.ValueObjects.Phone.Create(phone);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
