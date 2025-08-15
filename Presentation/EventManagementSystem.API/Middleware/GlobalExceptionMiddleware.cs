// <copyright file="GlobalExceptionMiddleware.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Middleware
{
    using System.Net;
    using System.Text.Json;
    using EventManagementSystem.Application.Common.Models;

    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<GlobalExceptionMiddleware> logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await this.next(context);
            }
            catch (Exception ex)
            {
                await this.HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            this.logger.LogError(exception, "An error occurred: {Message}", exception.Message);

            var (statusCode, errors) = exception switch
            {
                Application.Common.Exceptions.ValidationException validationEx =>
                    (HttpStatusCode.BadRequest, validationEx.Failures.Select(f => Error.Validation("Validation", f))),

                Application.Common.Exceptions.NotFoundException =>
                    (HttpStatusCode.NotFound, new[] { Error.NotFound("NotFound", "The requested resource was not found") }),

                Application.Common.Exceptions.ForbiddenException =>
                    (HttpStatusCode.Forbidden, new[] { Error.Forbidden("Forbidden", "Access to this resource is forbidden") }),

                Application.Common.Exceptions.ConflictException conflictEx =>
                    (HttpStatusCode.Conflict, new[] { Error.Conflict("Conflict", conflictEx.Message) }),

                UnauthorizedAccessException =>
                    (HttpStatusCode.Unauthorized, new[] { Error.Unauthorized("Unauthorized", "Authentication required") }),

                _ => (HttpStatusCode.InternalServerError, new[] { Error.Failure("ServerError", "An internal server error occurred") })
            };

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                errors = errors.Select(e => new { code = e.Code, message = e.Message, type = e.Type.ToString() }),
                timestamp = DateTime.UtcNow,
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
