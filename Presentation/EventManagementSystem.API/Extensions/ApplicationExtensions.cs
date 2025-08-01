// <copyright file="ApplicationExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Extensions
{
    using EventManagementSystem.API.Endpoints;
    using EventManagementSystem.API.Middleware;
    using EventManagementSystem.Identity;
    using EventManagementSystem.Persistence;

    public static class ApplicationExtensions
    {
        public static async Task<WebApplication> ConfigureApplicationAsync(this WebApplication app)
        {
            // Configure middleware pipeline
            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseMiddleware<SecurityHeadersMiddleware>();
            app.UseMiddleware<RequestLoggingMiddleware>();

            // Development specific configuration
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Management API V1");
                    c.RoutePrefix = "swagger";
                    c.DisplayRequestDuration();
                    c.EnableDeepLinking();
                });
            }

            // Security and CORS - IMPORTANT: CORS must come before Authentication/Authorization
            app.UseHttpsRedirection();

            // Use CORS policy based on environment
            if (app.Environment.IsDevelopment())
            {
                app.UseCors("Development"); // More permissive for development
            }
            else
            {
                app.UseCors("Production");
            }

            // Rate limiting
            app.UseRateLimiter();

            // Authentication & Authorization - MUST come after CORS
            app.UseAuthentication();
            app.UseAuthorization();

            // Map endpoints
            app.MapAuthenticationEndpoints();
            app.MapEventEndpoints();
            app.MapCategoryEndpoints();
            app.MapRegistrationEndpoints();
            app.MapUserEndpoints();
            app.MapAdminEndpoints();

            app.MapGet("/", () => Results.Ok(new
            {
                Message = "Event Management System API",
                Version = "1.0.0",
                Swagger = "/swagger",
                Health = "/health",
                Timestamp = DateTime.UtcNow,
            }))
            .WithTags("Root")
            .AllowAnonymous();

            // Health checks
            app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }))
                .WithTags("Health")
                .AllowAnonymous();

            // Initialize database and identity
            using (var scope = app.Services.CreateScope())
            {
                await scope.ServiceProvider.InitializeDatabaseAsync();
                await scope.ServiceProvider.InitializeIdentityAsync();
            }

            return app;
        }
    }
}