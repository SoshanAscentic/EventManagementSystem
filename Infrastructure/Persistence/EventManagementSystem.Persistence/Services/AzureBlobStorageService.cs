// <copyright file="AzureBlobStorageService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Services
{
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Persistence.Configurations;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobServiceClient blobServiceClient;
        private readonly AzureBlobStorageSettings settings;
        private readonly ILogger<AzureBlobStorageService> logger;

        public AzureBlobStorageService(
            BlobServiceClient blobServiceClient,
            IOptions<AzureBlobStorageSettings> options,
            ILogger<AzureBlobStorageService> logger)
        {
            this.blobServiceClient = blobServiceClient;
            this.settings = options.Value;
            this.logger = logger;
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Uploading file: {FileName}", fileName);

                // Generate unique file name to prevent conflicts
                var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                var blobPath = $"events/{DateTime.UtcNow:yyyy/MM/dd}/{uniqueFileName}";

                var containerClient = this.blobServiceClient.GetBlobContainerClient(this.settings.BlobContainerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

                var blobClient = containerClient.GetBlobClient(blobPath);

                // Set blob options
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType,
                };

                var blobUploadOptions = new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                    Metadata = new Dictionary<string, string>
                    {
                        ["OriginalFileName"] = fileName,
                        ["UploadedAt"] = DateTime.UtcNow.ToString("O"),
                        ["ContentType"] = contentType,
                    },
                };

                // Upload the file
                await blobClient.UploadAsync(fileStream, blobUploadOptions, cancellationToken);

                this.logger.LogInformation("Successfully uploaded file: {FileName} to {BlobPath}", fileName, blobPath);
                return blobPath;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to upload file: {FileName}", fileName);
                throw new InvalidOperationException($"Failed to upload file: {fileName}", ex);
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Deleting file: {FilePath}", filePath);

                var containerClient = this.blobServiceClient.GetBlobContainerClient(this.settings.BlobContainerName);
                var blobClient = containerClient.GetBlobClient(filePath);

                var response = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);

                this.logger.LogInformation("File deletion result for {FilePath}: {Deleted}", filePath, response.Value);
                return response.Value;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to delete file: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Retrieving file: {FilePath}", filePath);

                var containerClient = this.blobServiceClient.GetBlobContainerClient(this.settings.BlobContainerName);
                var blobClient = containerClient.GetBlobClient(filePath);

                if (!await blobClient.ExistsAsync(cancellationToken))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
                return response.Value.Content;
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                this.logger.LogError(ex, "Failed to retrieve file: {FilePath}", filePath);
                throw new InvalidOperationException($"Failed to retrieve file: {filePath}", ex);
            }
        }

        public async Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var containerClient = this.blobServiceClient.GetBlobContainerClient(this.settings.BlobContainerName);
                var blobClient = containerClient.GetBlobClient(filePath);

                var response = await blobClient.ExistsAsync(cancellationToken);
                return response.Value;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to check file existence: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<string> GetFileUrlAsync(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                var containerClient = this.blobServiceClient.GetBlobContainerClient(this.settings.BlobContainerName);
                var blobClient = containerClient.GetBlobClient(filePath);

                if (!await blobClient.ExistsAsync(cancellationToken))
                {
                    throw new FileNotFoundException($"File not found: {filePath}");
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                this.logger.LogError(ex, "Failed to get file URL: {FilePath}", filePath);
                throw new InvalidOperationException($"Failed to get file URL: {filePath}", ex);
            }
        }
    }
}
