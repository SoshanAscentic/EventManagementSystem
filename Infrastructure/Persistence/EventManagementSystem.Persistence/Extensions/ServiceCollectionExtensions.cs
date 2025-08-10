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
