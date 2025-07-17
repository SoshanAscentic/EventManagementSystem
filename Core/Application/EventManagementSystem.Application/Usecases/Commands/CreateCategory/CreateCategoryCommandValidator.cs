// <copyright file="CreateCategoryCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CreateCategory
{
    using FluentValidation;

    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            this.RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);

            this.RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(500);
        }
    }
}
