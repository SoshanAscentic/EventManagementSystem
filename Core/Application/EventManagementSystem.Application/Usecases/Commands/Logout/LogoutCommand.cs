// <copyright file="LogoutCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.Logout
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class LogoutCommand : IRequest<Result>
    {
        public string RefreshToken { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;
    }
}
