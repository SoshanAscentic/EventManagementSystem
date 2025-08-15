// <copyright file="DependencyInjection.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Utils
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Utils.Services;
    using Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Only register service implementations, not ASP.NET Core features
            // SignalR configuration stays in the API layer
            services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();

            // Override the basic notification service with the enhanced one
            services.AddScoped<INotificationService, EnhancedNotificationService>();

            // Register the background service for scheduled notifications
            services.AddHostedService<NotificationBackgroundService>();

            return services;
        }
    }
}
