// <copyright file="UpdateEventCapacityCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UpdateEventCapacity
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class UpdateEventCapacityCommand : IRequest<Result>
    {
        public int EventId { get; set; }

        public int NewCapacity { get; set; }
    }
}
