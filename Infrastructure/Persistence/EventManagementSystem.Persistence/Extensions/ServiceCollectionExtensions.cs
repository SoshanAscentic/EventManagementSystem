// <copyright file="ServiceCollectionExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Extensions
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public static class ServiceCollectionExtensions
    {
        //public static IServiceCollection AddHealthChecks(this IServiceCollection services, string connectionString)
        //{
        //    services.AddHealthChecks()
        //        .AddSqlServer(connectionString, tags: new[] { "database" })
        //        .AddAzureBlobStorage(connectionString, tags: new[] { "blob-storage" });

        //    return services;
        //}

        public static async Task<IHost> MigrateAndSeedDatabaseAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<IHost>>();

            try
            {
                await host.Services.InitializeDatabaseAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to migrate and seed database");
                throw;
            }

            return host;
        }
    }
}
