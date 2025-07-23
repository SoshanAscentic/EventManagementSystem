// <copyright file="RegistrationEndpoints.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Endpoints
{
    using EventManagementSystem.API.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Application.Usecases.Commands.CancelRegistration;
    using EventManagementSystem.Application.Usecases.Commands.MarkAttendance;
    using EventManagementSystem.Application.Usecases.Commands.RegisterForEvent;
    using EventManagementSystem.Application.Usecases.Queries.GetEventRegistration;
    using EventManagementSystem.Application.Usecases.Queries.GetUserRegistrations;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public static class RegistrationEndpoints
    {
        public static void MapRegistrationEndpoints(this IEndpointRouteBuilder app)
        {
            var registrations = app.MapGroup("/api/registrations")
                .WithTags("Registrations")
                .RequireRateLimiting("Api");

            // User endpoints
            registrations.MapPost("/", RegisterForEventAsync)
                .WithName("RegisterForEvent")
                .WithSummary("Register for an event")
                .WithDescription("Registers the current user for an event")
                .RequireAuthorization()
                .Produces<ApiResponse<int>>(201)
                .Produces<ApiResponse>(400)
                .Produces<ApiResponse>(401);

            registrations.MapDelete("/{registrationId:int}", CancelRegistrationAsync)
                .WithName("CancelRegistration")
                .WithSummary("Cancel a registration")
                .WithDescription("Cancels a user's registration for an event")
                .RequireAuthorization()
                .Produces<ApiResponse>(200)
                .Produces<ApiResponse>(400)
                .Produces<ApiResponse>(401);

            registrations.MapGet("/my-registrations", GetMyRegistrationsAsync)
                .WithName("GetMyRegistrations")
                .WithSummary("Get current user's registrations")
                .WithDescription("Retrieves all registrations for the current user")
                .RequireAuthorization()
                .Produces<ApiResponse<PagedResponse<RegistrationDto>>>(200)
                .Produces<ApiResponse>(401);

            // Admin endpoints
            registrations.MapGet("/event/{eventId:int}", GetEventRegistrationsAsync)
                .WithName("GetEventRegistrations")
                .WithSummary("Get event registrations")
                .WithDescription("Retrieves all registrations for a specific event (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse<PagedResponse<RegistrationDto>>>(200)
                .Produces<ApiResponse>(403);

            registrations.MapPut("/{registrationId:int}/attendance", MarkAttendanceAsync)
                .WithName("MarkAttendance")
                .WithSummary("Mark attendance")
                .WithDescription("Marks attendance for a registration (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse>(200)
                .Produces<ApiResponse>(400)
                .Produces<ApiResponse>(403);
        }

        private static async Task<IResult> RegisterForEventAsync(
            [FromBody] RegisterForEventRequest request,
            ISender mediator,
            HttpContext context)
        {
            var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var command = new RegisterForEventCommand
            {
                EventId = request.EventId,
                UserId = userId,
                Notes = request.Notes,
            };

            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Created(
                $"/api/registrations/{result.Value}",
                ApiResponse<int>.SuccessResponse(result.Value, "Registration successful"));
        }

        private static async Task<IResult> CancelRegistrationAsync(
            int registrationId,
            [FromBody] CancelRegistrationRequest? request,
            ISender mediator)
        {
            var command = new CancelRegistrationCommand
            {
                RegistrationId = registrationId,
                Reason = request?.Reason,
            };

            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse.SuccessResponse("Registration cancelled successfully"));
        }

        private static async Task<IResult> GetMyRegistrationsAsync(
            [AsParameters] PaginationParameters pagination,
            ISender mediator,
            HttpContext context)
        {
            var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var query = new GetUserRegistrationsQuery(userId)
            {
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
            };

            var result = await mediator.Send(query);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            var response = new PagedResponse<RegistrationDto>
            {
                Items = result.Value.Items,
                TotalCount = result.Value.TotalCount,
                PageNumber = result.Value.PageNumber,
                PageSize = result.Value.PageSize,
                TotalPages = result.Value.TotalPages,
                HasNextPage = result.Value.HasNextPage,
                HasPreviousPage = result.Value.HasPreviousPage,
            };

            return Results.Ok(ApiResponse<PagedResponse<RegistrationDto>>.SuccessResponse(response));
        }

        private static async Task<IResult> GetEventRegistrationsAsync(
            int eventId,
            [AsParameters] PaginationParameters pagination,
            [FromQuery] string? status,
            ISender mediator)
        {
            var query = new GetEventRegistrationsQuery(eventId)
            {
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                Status = status,
            };

            var result = await mediator.Send(query);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            var response = new PagedResponse<RegistrationDto>
            {
                Items = result.Value.Items,
                TotalCount = result.Value.TotalCount,
                PageNumber = result.Value.PageNumber,
                PageSize = result.Value.PageSize,
                TotalPages = result.Value.TotalPages,
                HasNextPage = result.Value.HasNextPage,
                HasPreviousPage = result.Value.HasPreviousPage,
            };

            return Results.Ok(ApiResponse<PagedResponse<RegistrationDto>>.SuccessResponse(response));
        }

        private static async Task<IResult> MarkAttendanceAsync(
            int registrationId,
            [FromBody] MarkAttendanceRequest request,
            ISender mediator)
        {
            var command = new MarkAttendanceCommand
            {
                RegistrationId = registrationId,
                Attended = request.Attended,
            };

            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse.SuccessResponse("Attendance marked successfully"));
        }
    }
}
