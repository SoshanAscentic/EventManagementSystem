// <copyright file="UpdateUserProfileCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UpdateUserProfile
{
    using FluentValidation;

    public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
    {
        public UpdateUserProfileCommandValidator()
        {
            this.RuleFor(x => x.UserId)
                .GreaterThan(0);

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
