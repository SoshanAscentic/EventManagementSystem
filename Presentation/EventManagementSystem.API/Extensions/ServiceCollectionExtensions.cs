// <copyright file="ServiceCollectionExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Extensions
{
    using System.Reflection;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Utils.Services;
    using Microsoft.AspNetCore.RateLimiting;
    using Microsoft.OpenApi.Models;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddHttpContextAccessor();

            // CORS Configuration - Fixed with correct ports
            services.AddCors(options =>
            {
                options.AddPolicy("Development", policy =>
                {
                    policy.WithOrigins(
                              "http://localhost:5173",   // Vite default port
                              "https://localhost:5173",  // HTTPS version
                              "http://localhost:5174",   // Alternative port
                              "https://localhost:5174",  // HTTPS alternative
                              "http://localhost:3000",   // React/Next.js default
                              "https://localhost:3000") // HTTPS React/Next.js

                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials() // Allow credentials for auth
                          .SetIsOriginAllowedToAllowWildcardSubdomains(); // Allow subdomains
                });

                options.AddPolicy("Production", policy =>
                {
                    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                        ?? new[] { "https://yourdomain.com" };

                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });

                // Fallback policy for any environment
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                              "http://localhost:5173",
                              "https://localhost:5173",
                              "http://localhost:5174",
                              "https://localhost:5174",
                              "http://localhost:3000",
                              "https://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            return services;
        }

        public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Event Management System API",
                    Version = "v1",
                    Description = "A comprehensive event management system API with real-time features",
                    Contact = new OpenApiContact
                    {
                        Name = "Event Management Team",
                        Email = "support@eventmanagement.com",
                    },
                });

                // JWT Authentication - Fixed
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    },
                });

                // Include XML comments if available
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            return services;
        }

        public static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            services.AddSignalRWithAuth();
            services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
            return services;
        }

        public static IServiceCollection AddRateLimitingServices(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("Api", configure =>
                {
                    configure.PermitLimit = 100;
                    configure.Window = TimeSpan.FromMinutes(1);
                    configure.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    configure.QueueLimit = 10;
                });

                options.AddFixedWindowLimiter("Auth", configure =>
                {
                    configure.PermitLimit = 10;
                    configure.Window = TimeSpan.FromMinutes(1);
                    configure.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    configure.QueueLimit = 2;
                });

                options.AddFixedWindowLimiter("Upload", configure =>
                {
                    configure.PermitLimit = 5;
                    configure.Window = TimeSpan.FromMinutes(1);
                    configure.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
                    configure.QueueLimit = 2;
                });
            });

            return services;
        }
    }
}
