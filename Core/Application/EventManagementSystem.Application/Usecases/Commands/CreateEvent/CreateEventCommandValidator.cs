// <copyright file="CreateEventCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CreateEvent
{
    using EventManagementSystem.Domain.ValueObjects;
    using FluentValidation;

    public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
    {
        public CreateEventCommandValidator()
        {
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

            RuleFor(x => x.Capacity)
                .GreaterThan(0)
                .LessThanOrEqualTo(10000);

            RuleFor(x => x.EventType)
                .NotEmpty()
                .Must(BeValidEventType)
                .WithMessage("Invalid event type");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0);
        }

        private static bool BeValidEventType(string eventType)
        {
            try
            {
                EventType.Create(eventType);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
