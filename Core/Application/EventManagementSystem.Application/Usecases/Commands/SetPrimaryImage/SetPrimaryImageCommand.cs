// <copyright file="SetPrimaryImageCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.SetPrimaryImage
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class SetPrimaryImageCommand : IRequest<Result>
    {
        public int EventId { get; set; }

        public int ImageId { get; set; }
    }
}
