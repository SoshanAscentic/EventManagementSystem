// <copyright file="DeleteImageCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.DeleteImage
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class DeleteEventImageCommand : IRequest<Result>
    {
        public int EventId { get; set; }

        public int ImageId { get; set; }
    }
}
