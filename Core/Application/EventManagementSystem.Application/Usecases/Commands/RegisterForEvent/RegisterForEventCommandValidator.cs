// <copyright file="RegisterForEventCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.RegisterForEvent
{
    using FluentValidation;

    public class RegisterForEventCommandValidator : AbstractValidator<RegisterForEventCommand>
    {
        public RegisterForEventCommandValidator()
        {
            this.RuleFor(x => x.EventId)
                .GreaterThan(0);

            this.RuleFor(x => x.UserId)
                .GreaterThan(0);

            this.RuleFor(x => x.Notes)
                .MaximumLength(500)
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}
