// <copyright file="Program.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

using EventManagementSystem.API.Extensions;
using EventManagementSystem.Application;
using EventManagementSystem.Identity;
using EventManagementSystem.Persistence;
using Serilog;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Serilog
        builder.Host.UseSerilog((context, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
        });

        try
        {
            Log.Information("Starting Event Management System API");

            // Add services to the container
            builder.Services.AddApplication();
            builder.Services.AddPersistence(builder.Configuration);
            builder.Services.AddIdentityServices(builder.Configuration);

            // Add API services
            builder.Services.AddApiServices(builder.Configuration);
            builder.Services.AddAuthenticationServices(builder.Configuration);
            builder.Services.AddSwaggerServices();
            builder.Services.AddSignalRServices();
            builder.Services.AddRateLimitingServices();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            await app.ConfigureApplicationAsync();

            Log.Information("Event Management System API started successfully");
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
