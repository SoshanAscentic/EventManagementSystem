namespace EventManagementSystem.Persistence.Configurations;

public class AzureBlobStorageSettings
{
    public const string SectionName = "AzureBlobStorage";
    
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
} 