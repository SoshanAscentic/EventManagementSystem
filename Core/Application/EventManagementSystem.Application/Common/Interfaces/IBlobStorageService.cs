// <copyright file="IBlobStorageService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Interfaces
{
    using Microsoft.AspNetCore.Http;

    public interface IBlobStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string containerName, string? fileName = null);

        Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName, string contentType);

        Task<bool> DeleteFileAsync(string fileName, string containerName);

        Task<Stream> DownloadFileAsync(string fileName, string containerName);

        Task<string> GetFileUrlAsync(string fileName, string containerName);

        Task<bool> FileExistsAsync(string fileName, string containerName);
    }
}
