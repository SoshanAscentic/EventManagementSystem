// <copyright file="DatabaseConfiguration.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Configurations
{
    using EventManagementSystem.Persistence.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class DatabaseConfiguration
    {
        public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    // Connection resilience
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);

                    // Query timeout
                    sqlOptions.CommandTimeout(30);

                    // Migration assembly
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);

                    // Performance optimizations
                    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });

                // Development-only configurations
                if (configuration.GetValue<bool>("DetailedErrors"))
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }

                // Query optimization
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            // Add read-only context for queries if needed - Fixed the overload issue
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var readOnlyConnectionString = configuration.GetConnectionString("ReadOnlyConnection") ?? connectionString;
                options.UseSqlServer(readOnlyConnectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure();
                    sqlOptions.CommandTimeout(60); // Longer timeout for read operations
                });

                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
        }

        public static void ConfigureAzureStorage(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AzureStorage");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Azure Storage connection string is required");
            }

            services.Configure<AzureStorageOptions>(options =>
            {
                options.ConnectionString = connectionString;
                options.ContainerName = configuration["AzureStorage:ContainerName"] ?? "event-images";
                options.MaxFileSize = configuration.GetValue<long>("AzureStorage:MaxFileSize", 10485760); // 10MB
                options.AllowedFileTypes = configuration.GetSection("AzureStorage:AllowedFileTypes")
                    .Get<string[]>() ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            });
        }
    }

    public class AzureStorageOptions
    {
        public string ConnectionString { get; set; } = string.Empty;

        public string ContainerName { get; set; } = "event-images";

        public long MaxFileSize { get; set; } = 10485760; // 10MB

        public string[] AllowedFileTypes { get; set; } = Array.Empty<string>();
    }
}