// <copyright file="CreateUserCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CreateUser
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class CreateUserCommand : IRequest<Result<int>>
    {
        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Phone { get; set; }
    }
}
