// <copyright file="RegisterForEventCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.RegisterForEvent
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class RegisterForEventCommand : IRequest<Result<int>>
    {
        public int EventId { get; set; }

        public int UserId { get; set; }

        public string? Notes { get; set; }
    }
}
