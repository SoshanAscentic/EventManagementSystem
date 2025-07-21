// <copyright file="MigrationExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Extensions
{
    using EventManagementSystem.Persistence.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public static class MigrationExtensions
    {
        public static async Task<IHost> MigrateAndSeedAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<IHost>>();

            try
            {
                logger.LogInformation("Starting database migration and seeding");

                var context = services.GetRequiredService<ApplicationDbContext>();

                // Apply migrations
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    logger.LogInformation("Applying {Count} pending migrations", pendingMigrations.Count());
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully");
                }
                else
                {
                    logger.LogInformation("No pending migrations found");
                }

                // Seed data
                var seeder = services.GetRequiredService<DatabaseSeeder>();
                await seeder.SeedAsync();

                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while migrating or seeding the database");
                throw;
            }

            return host;
        }
    }
}
