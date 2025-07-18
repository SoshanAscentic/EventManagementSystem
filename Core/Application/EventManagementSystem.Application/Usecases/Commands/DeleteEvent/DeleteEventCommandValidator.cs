// <copyright file="DeleteEventCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.DeleteEvent
{
    using FluentValidation;

    public class DeleteEventCommandValidator : AbstractValidator<DeleteEventCommand>
    {
        public DeleteEventCommandValidator()
        {
            this.RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Event ID must be a positive integer");
        }
    }
}
