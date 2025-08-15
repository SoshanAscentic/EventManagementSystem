// <copyright file="UpdateEventCapacityCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UpdateEventCapacity
{
    using FluentValidation;

    public class UpdateEventCapacityCommandValidator : AbstractValidator<UpdateEventCapacityCommand>
    {
        public UpdateEventCapacityCommandValidator()
        {
            this.RuleFor(x => x.EventId)
                .GreaterThan(0);

            this.RuleFor(x => x.NewCapacity)
                .GreaterThan(0)
                .LessThanOrEqualTo(10000);
        }
    }
}
