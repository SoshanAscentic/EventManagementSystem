// <copyright file="FileUploadRequest.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.DTOs
{
    using Microsoft.AspNetCore.Http;

    public class FileUploadRequest
    {
        public IFormFile File { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsPrimary { get; set; }
    }

    public class FileUploadResponse
    {
        public string FileName { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public long Size { get; set; }

        public string ContentType { get; set; } = string.Empty;
    }
}
