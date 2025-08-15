// <copyright file="MarkAttendanceCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.MarkAttendance
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class MarkAttendanceCommand : IRequest<Result>
    {
        public int RegistrationId { get; set; }

        public bool Attended { get; set; }
    }
}
