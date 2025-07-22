// <copyright file="RefreshTokenCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.RefreshToken
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;

    public class RefreshTokenCommand : IRequest<Result<AuthenticationResponse>>
    {
        public string RefreshToken { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;
    }
}
