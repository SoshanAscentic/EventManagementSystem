// <copyright file="Program.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>
using DotNetEnv;
using EventManagementSystem.API.Endpoints;
using EventManagementSystem.API.Extensions;
using EventManagementSystem.Application;
using EventManagementSystem.Application.Common.Interfaces;
using EventManagementSystem.Identity;
using EventManagementSystem.Persistence;
using EventManagementSystem.Utils;
using Microsoft.AspNetCore.Http.Json;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Env.Load();

        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables();

        // Configure Serilog
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
        });

        builder.Services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        try
        {
            Log.Information("Starting Event Management System API");

            // Add layers in proper dependency order
            // Add core layers first
            builder.Services.AddApplication(); // Application layer (interfaces + basic implementations)
            builder.Services.AddPersistence(builder.Configuration); // Data access layer
            builder.Services.AddIdentityServices(builder.Configuration); // Identity layer

            // Infrastructure layer (overrides application services with enhanced implementations)
            builder.Services.AddInfrastructureServices();

            // Add API services (these stay the same)
            builder.Services.AddApiServices(builder.Configuration);
            builder.Services.AddAuthenticationServices(builder.Configuration);
            builder.Services.AddSwaggerServices();
            builder.Services.AddSignalRServices(); // This handles SignalR ASP.NET Core configuration
            builder.Services.AddRateLimitingServices();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            app.UseStaticFiles();
            await app.ConfigureApplicationAsync();

            // Map SignalR hubs
            app.MapSignalRHubs(); // This maps the SignalR hubs

            Log.Information("Event Management System API started successfully on {Environment}", app.Environment.EnvironmentName);
            Log.Information("CORS policies configured for environment: {Environment}", app.Environment.EnvironmentName);
            Log.Information("SignalR hub mapped at /notificationHub");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
