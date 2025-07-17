// <copyright file="DeleteEventCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.DeleteEvent
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class DeleteEventCommand : IRequest<Result>
    {
        public DeleteEventCommand(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}
