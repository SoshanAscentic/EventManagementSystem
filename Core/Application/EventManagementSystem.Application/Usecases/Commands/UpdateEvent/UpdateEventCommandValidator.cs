// <copyright file="UpdateEventCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UpdateEvent
{
    using FluentValidation;

    public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
    {
        public UpdateEventCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0);

            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(2000);

            RuleFor(x => x.StartDateTime)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Event start time must be in the future");

            RuleFor(x => x.EndDateTime)
                .GreaterThan(x => x.StartDateTime)
                .WithMessage("Event end time must be after start time");

            RuleFor(x => x.Venue)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Address)
                .NotEmpty()
                .MaximumLength(200);
        }
    }
}
