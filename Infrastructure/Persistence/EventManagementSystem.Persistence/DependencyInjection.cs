// <copyright file="DependencyInjection.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence
{
    using Azure.Storage.Blobs;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Persistence.Context;
    using EventManagementSystem.Persistence.Interceptors;
    using EventManagementSystem.Persistence.Repositories;
    using EventManagementSystem.Persistence.Services;
    using EventManagementSystem.Persistence.UoW;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            // Register interceptors
            services.AddScoped<AuditInterceptor>();
            services.AddScoped<DomainEventInterceptor>();

            // Database Context
            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);

                    sqlOptions.CommandTimeout(30);
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                });

                // Add interceptors
                options.AddInterceptors(
                    serviceProvider.GetRequiredService<AuditInterceptor>(),
                    serviceProvider.GetRequiredService<DomainEventInterceptor>());

                // Enable detailed errors in development
                if (configuration.GetValue<bool>("DetailedErrors"))
                {
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                }
            });

            // Application DbContext Interface
            services.AddScoped<IApplicationDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repositories
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventCategoryRepository, EventCategoryRepository>();
            services.AddScoped<IEventRegistrationRepository, EventRegistrationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            // Services
            services.AddScoped<IStatisticsService, StatisticsService>();
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
            services.AddScoped<ICacheService, CacheService>();

            // Azure Blob Storage
            services.AddSingleton(serviceProvider =>
            {
                var connectionString = configuration.GetConnectionString("AzureStorage");
                return new BlobServiceClient(connectionString);
            });

            // Cache Service
            services.AddMemoryCache();

            // Database Seeder
            services.AddScoped<DatabaseSeeder>();

            return services;
        }

        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                logger.LogInformation("Applying database migrations");
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");

                // Seed initial data
                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                await seeder.SeedAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while initializing database");
                throw;
            }
        }
    }
}
