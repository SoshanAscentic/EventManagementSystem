// <copyright file="EventEndpoints.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Endpoints
{
    using EventManagementSystem.API.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Application.Usecases.Commands.CreateEvent;
    using EventManagementSystem.Application.Usecases.Commands.DeleteEvent;
    using EventManagementSystem.Application.Usecases.Commands.UpdateEvent;
    using EventManagementSystem.Application.Usecases.Commands.UploadEventImage;
    using EventManagementSystem.Application.Usecases.Queries.GetEvent;
    using EventManagementSystem.Application.Usecases.Queries.GetEvents;
    using EventManagementSystem.Application.Usecases.Queries.GetUpcomingEvent;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public static class EventEndpoints
    {
        public static void MapEventEndpoints(this IEndpointRouteBuilder app)
        {
            var events = app.MapGroup("/api/events")
                .WithTags("Events")
                .RequireRateLimiting("Api");

            // Public endpoints
            events.MapGet("/", GetEventsAsync)
                .WithName("GetEvents")
                .WithSummary("Get events with filtering and pagination")
                .WithDescription("Retrieves a paginated list of events with optional filtering")
                .Produces<ApiResponse<PagedResponse<EventDto>>>(200);

            events.MapGet("/{id:int}", GetEventByIdAsync)
                .WithName("GetEvent")
                .WithSummary("Get event by ID")
                .WithDescription("Retrieves detailed information about a specific event")
                .Produces<ApiResponse<EventDto>>(200)
                .Produces<ApiResponse>(404);

            events.MapGet("/upcoming", GetUpcomingEventsAsync)
                .WithName("GetUpcomingEvents")
                .WithSummary("Get upcoming events")
                .WithDescription("Retrieves a list of upcoming events")
                .Produces<ApiResponse<List<EventDto>>>(200);

            // Admin only endpoints
            events.MapPost("/", CreateEventAsync)
                .WithName("CreateEvent")
                .WithSummary("Create a new event")
                .WithDescription("Creates a new event (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse<EventDto>>(201)
                .Produces<ApiResponse>(400)
                .Produces<ApiResponse>(403);

            events.MapPut("/{id:int}", UpdateEventAsync)
                .WithName("UpdateEvent")
                .WithSummary("Update an event")
                .WithDescription("Updates an existing event (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse>(200)
                .Produces<ApiResponse>(400)
                .Produces<ApiResponse>(404)
                .Produces<ApiResponse>(403);

            events.MapDelete("/{id:int}", DeleteEventAsync)
                .WithName("DeleteEvent")
                .WithSummary("Delete an event")
                .WithDescription("Deletes an event (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse>(200)
                .Produces<ApiResponse>(404)
                .Produces<ApiResponse>(403);

            events.MapPost("/{id:int}/images", UploadEventImageAsync)
                .WithName("UploadEventImage")
                .WithSummary("Upload event image")
                .WithDescription("Uploads an image for an event (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse<int>>(201)
                .Produces<ApiResponse>(400)
                .Produces<ApiResponse>(403)
                .DisableAntiforgery();
        }

        private static async Task<IResult> GetEventsAsync(
            [AsParameters] PaginationParameters pagination,
            [FromQuery] int? categoryId,
            [FromQuery] string? eventType,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? location,
            [FromQuery] bool? hasAvailableSpots,
            ISender mediator)
        {
            var query = new GetEventsQuery
            {
                SearchTerm = pagination.SearchTerm,
                CategoryId = categoryId,
                EventType = eventType,
                StartDate = startDate,
                EndDate = endDate,
                Location = location,
                HasAvailableSpots = hasAvailableSpots,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                SortBy = pagination.SortBy ?? "StartDateTime",
                Ascending = pagination.Ascending
            };

            var result = await mediator.Send(query);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            var response = new PagedResponse<EventDto>
            {
                Items = result.Value.Items,
                TotalCount = result.Value.TotalCount,
                PageNumber = result.Value.PageNumber,
                PageSize = result.Value.PageSize,
                TotalPages = result.Value.TotalPages,
                HasNextPage = result.Value.HasNextPage,
                HasPreviousPage = result.Value.HasPreviousPage
            };

            return Results.Ok(ApiResponse<PagedResponse<EventDto>>.SuccessResponse(response));
        }

        private static async Task<IResult> GetEventByIdAsync(
            int id,
            ISender mediator)
        {
            var query = new GetEventQuery(id);
            var result = await mediator.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse<EventDto>.SuccessResponse(result.Value));
        }

        private static async Task<IResult> GetUpcomingEventsAsync(
            [FromQuery] int? categoryId,
            [FromQuery] int count = 10,
            ISender mediator)
        {
            var query = new GetUpcomingEventsQuery
            {
                CategoryId = categoryId,
                Count = count
            };

            var result = await mediator.Send(query);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse<List<EventDto>>.SuccessResponse(result.Value));
        }

        private static async Task<IResult> CreateEventAsync(
            [FromBody] CreateEventCommand command,
            ISender mediator)
        {
            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Created($"/api/events/{result.Value.Id}",
                ApiResponse<EventDto>.SuccessResponse(result.Value, "Event created successfully"));
        }

        private static async Task<IResult> UpdateEventAsync(
            int id,
            [FromBody] UpdateEventCommand command,
            ISender mediator)
        {
            command.Id = id;
            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse.SuccessResponse("Event updated successfully"));
        }

        private static async Task<IResult> DeleteEventAsync(
            int id,
            ISender mediator)
        {
            var command = new DeleteEventCommand(id);
            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse.SuccessResponse("Event deleted successfully"));
        }

        private static async Task<IResult> UploadEventImageAsync(
            int id,
            IFormFile file,
            [FromQuery] bool isPrimary,
            ISender mediator)
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse("No file uploaded"));
            }

            var command = new UploadEventImageCommand
            {
                EventId = id,
                FileStream = file.OpenReadStream(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                IsPrimary = isPrimary
            };

            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Created($"/api/events/{id}/images/{result.Value}",
                ApiResponse<int>.SuccessResponse(result.Value, "Image uploaded successfully"));
        }
    }
}
