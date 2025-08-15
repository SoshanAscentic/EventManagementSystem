// <copyright file="UploadEventImageCommand.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UploadEventImage
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class UploadEventImageCommand : IRequest<Result<int>>
    {
        public int EventId { get; set; }

        public Stream FileStream { get; set; } = null!;

        public string FileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public bool IsPrimary { get; set; }
    }
}
