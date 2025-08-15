// <copyright file="RequestLoggingMiddleware.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Middleware
{
    using System.Diagnostics;

    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<RequestLoggingMiddleware> logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = Guid.NewGuid().ToString();

            // Add correlation ID to response headers for debugging
            context.Response.Headers.Append("X-Correlation-ID", correlationId);

            var request = context.Request;
            var userAgent = request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // Log request with correlation ID
            this.logger.LogInformation(
                "Request started: {CorrelationId} | {Method} {Path} | IP: {RemoteIpAddress} | UserAgent: {UserAgent}",
                correlationId,
                request.Method,
                request.Path,
                ipAddress,
                userAgent);

            try
            {
                await this.next(context);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Request failed: {CorrelationId} | {Method} {Path} | Error: {ErrorMessage}",
                    correlationId,
                    request.Method,
                    request.Path,
                    ex.Message);
                throw;
            }
            finally
            {
                stopwatch.Stop();

                var response = context.Response;
                var level = response.StatusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

                this.logger.Log(
                    level,
                    "Request completed: {CorrelationId} | {Method} {Path} | Status: {StatusCode} | Duration: {ElapsedMilliseconds}ms",
                    correlationId,
                    request.Method,
                    request.Path,
                    response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
