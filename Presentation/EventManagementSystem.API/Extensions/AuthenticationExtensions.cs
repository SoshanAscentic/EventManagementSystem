// <copyright file="AuthenticationExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Extensions
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;

    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure cookie settings
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.Name = "EventManagement.Auth";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                options.SlidingExpiration = true;
            });

            // Override JWT Bearer options to read from headers, cookies, AND query parameters (for SignalR)
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var originalOnMessageReceived = options.Events.OnMessageReceived;
                options.Events.OnMessageReceived = context =>
                {
                    var path = context.HttpContext.Request.Path;

                    // IMPORTANT: Handle SignalR connections
                    if (path.StartsWithSegments("/notificationHub"))
                    {
                        // For SignalR, token comes via query parameter
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                            return originalOnMessageReceived?.Invoke(context) ?? Task.CompletedTask;
                        }
                    }

                    // First, try to get token from Authorization header (for Swagger/API clients)
                    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                    {
                        context.Token = authHeader.Substring("Bearer ".Length).Trim();
                    }
                    // If no header token, try to get from cookie (for web clients)
                    else if (string.IsNullOrEmpty(context.Token))
                    {
                        context.Token = context.Request.Cookies["AccessToken"];
                    }

                    return originalOnMessageReceived?.Invoke(context) ?? Task.CompletedTask;
                };

                // Add debugging for authentication failures
                options.Events.OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var path = context.HttpContext.Request.Path;

                    if (path.StartsWithSegments("/notificationHub"))
                    {
                        logger.LogWarning("SignalR JWT Authentication failed for path {Path}: {Exception}", path, context.Exception.Message);
                    }
                    else
                    {
                        logger.LogWarning("JWT Authentication failed for path {Path}: {Exception}", path, context.Exception.Message);
                    }
                    return Task.CompletedTask;
                };

                options.Events.OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                    var userId = context.Principal?.FindFirst("sub")?.Value ??
                                context.Principal?.FindFirst("nameid")?.Value ??
                                context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    var path = context.HttpContext.Request.Path;

                    if (path.StartsWithSegments("/notificationHub"))
                    {
                        logger.LogInformation("SignalR JWT Token validated for user: {UserId}", userId);
                    }
                    else
                    {
                        logger.LogInformation("JWT Token validated for user: {UserId}", userId);
                    }
                    return Task.CompletedTask;
                };
            });

            return services;
        }
    }
}