// <copyright file="CreateCategoryCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CreateCategory
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class CreateCategoryCommand : IRequest<Result<int>>
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
