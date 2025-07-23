// <copyright file="ServiceCollectionExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Extensions
{
    using System.Reflection;
    using Microsoft.AspNetCore.RateLimiting;
    using Microsoft.OpenApi.Models;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddHttpContextAccessor();

            // CORS Configuration - Fixed
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                        ?? new[] { "http://localhost:3000", "https://localhost:3001", "https://localhost:7026" }; // Added your current origin

                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials(); // Important for HTTP-only cookies
                });

                // Add a more permissive policy for development/testing
                options.AddPolicy("Development", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
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
                    BearerFormat = "JWT"
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

        // Rest of the methods remain the same...
        public static IServiceCollection AddSignalRServices(this IServiceCollection services)
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
