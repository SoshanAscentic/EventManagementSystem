// <copyright file="SignalRExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Extensions
{
    using EventManagementSystem.Utils.Services;

    public static class SignalRExtensions
    {
        public static IServiceCollection AddSignalRWithAuth(this IServiceCollection services)
        {
            // Add SignalR with configuration
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true; // Enable for debugging
                options.MaximumReceiveMessageSize = 32 * 1024; // 32KB
                options.StreamBufferCapacity = 10;
                options.MaximumParallelInvocationsPerClient = 2;

                // Configure timeouts
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
            });

            return services;
        }

        public static WebApplication MapSignalRHubs(this WebApplication app)
        {
            // Map the NotificationHub
            app.MapHub<NotificationHub>("/notificationHub", options =>
            {
                // Configure transport options
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling |
                                   Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;

                // Configure authorization
                options.AuthorizationData.Add(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute());
            });

            return app;
        }
    }
}
