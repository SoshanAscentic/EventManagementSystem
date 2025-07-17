// <copyright file="UpdateUserProfileCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UpdateUserProfile
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class UpdateUserProfileCommand : IRequest<Result>
    {
        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string? Phone { get; set; }
    }
}
