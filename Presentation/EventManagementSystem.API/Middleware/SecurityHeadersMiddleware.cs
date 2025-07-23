// <copyright file="SecurityHeadersMiddleware.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<SecurityHeadersMiddleware> logger;

        public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Remove server information
            context.Response.Headers.Remove("Server");

            // Add security headers
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
            context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");

            // Content Security Policy
            var csp = "default-src 'self'; " +
                     "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                     "style-src 'self' 'unsafe-inline'; " +
                     "img-src 'self' data: https: blob:; " +
                     "font-src 'self' data:; " +
                     "connect-src 'self' wss: ws: https:; " +
                     "media-src 'self'; " +
                     "object-src 'none'; " +
                     "child-src 'none'; " +
                     "frame-ancestors 'none'; " +
                     "form-action 'self'; " +
                     "base-uri 'self';";

            context.Response.Headers.Append("Content-Security-Policy", csp);

            // HSTS for HTTPS
            if (context.Request.IsHttps)
            {
                context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
            }

            await this.next(context);
        }
    }
}
