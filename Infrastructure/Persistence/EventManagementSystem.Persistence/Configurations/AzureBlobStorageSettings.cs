// <copyright file="AzureBlobStorageSettings.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Configurations;

public class AzureBlobStorageSettings
{
    public string BlobConnectionString { get; set; } = string.Empty;

    public string BlobContainerName { get; set; } = string.Empty;

    public string BlobBaseUrl { get; set; } = string.Empty;
} 
