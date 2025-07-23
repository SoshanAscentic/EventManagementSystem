namespace EventManagementSystem.API.Extensions
{
    using EventManagementSystem.Utils.Services;
    using Microsoft.AspNetCore.Http.Connections;

    public static class SignalRExtensions
    {
        public static IServiceCollection AddSignalRWithAuth(this IServiceCollection services)
        {
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromSeconds(30);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
                options.MaximumReceiveMessageSize = 32768; // 32KB
            });

            return services;
        }

        public static void MapSignalRHubs(this WebApplication app)
        {
            app.MapHub<NotificationHub>("/notificationHub", options =>
            {
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                                   Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
            });
        }
    }
}
