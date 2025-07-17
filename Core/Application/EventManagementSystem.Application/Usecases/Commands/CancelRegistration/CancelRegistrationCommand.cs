// <copyright file="CancelRegistrationCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CancelRegistration
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class CancelRegistrationCommand : IRequest<Result>
    {
        public int RegistrationId { get; set; }

        public string? Reason { get; set; }
    }
}
