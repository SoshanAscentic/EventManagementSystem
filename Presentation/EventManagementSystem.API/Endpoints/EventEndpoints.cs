// <copyright file="EventEndpoints.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Endpoints
{
    using EventManagementSystem.API.Models;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Application.Usecases.Commands.CreateEvent;
    using EventManagementSystem.Application.Usecases.Commands.DeleteEvent;
    using EventManagementSystem.Application.Usecases.Commands.DeleteImage;
    using EventManagementSystem.Application.Usecases.Commands.SetPrimaryImage;
    using EventManagementSystem.Application.Usecases.Commands.UpdateEvent;
    using EventManagementSystem.Application.Usecases.Commands.UpdateEventCapacity;
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

            events.MapPut("/{id:int}/capacity", UpdateEventCapacityAsync)
                .WithName("UpdateEventCapacity")
                .WithSummary("Update event capacity")
                .WithDescription("Updates the capacity of an event (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse>(200)
                .Produces<ApiResponse>(400)
                .Produces<ApiResponse>(403);

            events.MapDelete("/{id:int}", DeleteEventAsync)
                .WithName("DeleteEvent")
                .WithSummary("Delete an event")
                .WithDescription("Deletes an event (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse>(200)
                .Produces<ApiResponse>(404)
                .Produces<ApiResponse>(403);

            // Image management endpoints
            events.MapPost("/{id:int}/images", UploadEventImageAsync)
                .WithName("UploadEventImage")
                .WithSummary("Upload event image")
                .WithDescription("Uploads an image for an event (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .RequireRateLimiting("Upload")
                .Produces<ApiResponse<int>>(201)
                .Produces<ApiResponse>(400)
                .Produces<ApiResponse>(403)
                .DisableAntiforgery();

            events.MapPut("/{eventId:int}/images/{imageId:int}/primary", SetPrimaryImageAsync)
                .WithName("SetPrimaryImage")
                .WithSummary("Set primary image")
                .WithDescription("Sets an image as the primary image for an event (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse>(200)
                .Produces<ApiResponse>(400)
                .Produces<ApiResponse>(403);

            events.MapDelete("/{eventId:int}/images/{imageId:int}", DeleteEventImageAsync)
                .WithName("DeleteEventImage")
                .WithSummary("Delete event image")
                .WithDescription("Deletes an image from an event (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse>(200)
                .Produces<ApiResponse>(404)
                .Produces<ApiResponse>(403);
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
                Ascending = pagination.Ascending,
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
                HasPreviousPage = result.Value.HasPreviousPage,
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
            ISender mediator,
            [FromQuery] int? categoryId,
            [FromQuery] int count = 10)
        {
            var query = new GetUpcomingEventsQuery
            {
                CategoryId = categoryId,
                Count = count,
            };

            var result = await mediator.Send(query);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse<List<EventDto>>.SuccessResponse(result.Value));
        }

        // ✅ Updated CreateEventAsync to send notifications
        private static async Task<IResult> CreateEventAsync(
            [FromBody] CreateEventCommand command,
            ISender mediator,
            INotificationService notificationService, // ✅ Inject notification service
            ILogger<Program> logger)
        {
            try
            {
                // Log the incoming request for debugging
                logger.LogInformation("CreateEvent request received: {@Command}", new
                {
                    command.Title,
                    command.Description,
                    command.StartDateTime,
                    command.EndDateTime,
                    command.Venue,
                    command.Address,
                    command.City,
                    command.Country,
                    command.Capacity,
                    command.EventType,
                    command.CategoryId
                });

                // Validate the basic requirements
                if (string.IsNullOrWhiteSpace(command.Title))
                {
                    logger.LogWarning("CreateEvent failed: Title is required");
                    return Results.BadRequest(ApiResponse.ErrorResponse("Title is required"));
                }

                if (string.IsNullOrWhiteSpace(command.Description))
                {
                    logger.LogWarning("CreateEvent failed: Description is required");
                    return Results.BadRequest(ApiResponse.ErrorResponse("Description is required"));
                }

                if (command.StartDateTime <= DateTime.UtcNow)
                {
                    logger.LogWarning("CreateEvent failed: Start date must be in the future");
                    return Results.BadRequest(ApiResponse.ErrorResponse("Start date must be in the future"));
                }

                if (command.EndDateTime <= command.StartDateTime)
                {
                    logger.LogWarning("CreateEvent failed: End date must be after start date");
                    return Results.BadRequest(ApiResponse.ErrorResponse("End date must be after start date"));
                }

                if (command.Capacity <= 0)
                {
                    logger.LogWarning("CreateEvent failed: Capacity must be greater than 0");
                    return Results.BadRequest(ApiResponse.ErrorResponse("Capacity must be greater than 0"));
                }

                if (command.CategoryId <= 0)
                {
                    logger.LogWarning("CreateEvent failed: Valid category ID is required");
                    return Results.BadRequest(ApiResponse.ErrorResponse("Valid category ID is required"));
                }

                var result = await mediator.Send(command);

                if (result.IsFailure)
                {
                    logger.LogWarning("CreateEvent failed with errors: {@Errors}", result.GetErrorMessages());
                    return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
                }

                logger.LogInformation("Event created successfully with ID: {EventId}", result.Value.Id);

                // ✅ Send notification after successful creation
                try
                {
                    await notificationService.SendEventCreatedNotificationAsync(
                        result.Value.Id,
                        result.Value.Title);

                    logger.LogInformation("Event creation notification sent for: {EventTitle}", result.Value.Title);
                }
                catch (Exception notificationEx)
                {
                    // Don't fail the entire request if notification fails
                    logger.LogError(notificationEx, "Failed to send event creation notification for: {EventTitle}", result.Value.Title);
                }

                return Results.Created(
                    $"/api/events/{result.Value.Id}",
                    ApiResponse<EventDto>.SuccessResponse(result.Value, "Event created successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error creating event");
                return Results.Problem("An unexpected error occurred while creating the event");
            }
        }

        // ✅ Updated UpdateEventAsync to send notifications
        private static async Task<IResult> UpdateEventAsync(
            int id,
            [FromBody] UpdateEventCommand command,
            ISender mediator,
            INotificationService notificationService, // ✅ Inject notification service
            ILogger<Program> logger)
        {
            command.Id = id;
            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            // ✅ Send notification after successful update
            try
            {
                // Get the event title for notification (you might need to modify this based on your result structure)
                await notificationService.SendEventUpdatedNotificationAsync(id, command.Title);
                logger.LogInformation("Event update notification sent for event ID: {EventId}", id);
            }
            catch (Exception notificationEx)
            {
                logger.LogError(notificationEx, "Failed to send event update notification for event ID: {EventId}", id);
            }

            return Results.Ok(ApiResponse.SuccessResponse("Event updated successfully"));
        }

        // ✅ Updated DeleteEventAsync to send notifications
        private static async Task<IResult> DeleteEventAsync(
            int id,
            ISender mediator,
            INotificationService notificationService, // ✅ Inject notification service
            ILogger<Program> logger)
        {
            // Get event details before deletion for notification
            var getQuery = new GetEventQuery(id);
            var eventResult = await mediator.Send(getQuery);

            var command = new DeleteEventCommand(id);
            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            // ✅ Send cancellation notification after successful deletion
            if (eventResult.IsSuccess)
            {
                try
                {
                    await notificationService.SendEventCancelledNotificationAsync(id, eventResult.Value.Title);
                    logger.LogInformation("Event cancellation notification sent for: {EventTitle}", eventResult.Value.Title);
                }
                catch (Exception notificationEx)
                {
                    logger.LogError(notificationEx, "Failed to send event cancellation notification for: {EventTitle}", eventResult.Value.Title);
                }
            }

            return Results.Ok(ApiResponse.SuccessResponse("Event deleted successfully"));
        }

        private static async Task<IResult> UpdateEventCapacityAsync(
            int id,
            [FromBody] UpdateCapacityRequest request,
            ISender mediator)
        {
            var command = new UpdateEventCapacityCommand
            {
                EventId = id,
                NewCapacity = request.NewCapacity,
            };

            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse.SuccessResponse("Event capacity updated successfully"));
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

            // Validate file type
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return Results.BadRequest(ApiResponse.ErrorResponse("Invalid file type. Only JPEG, PNG, GIF, and WebP images are allowed."));
            }

            // Validate file size (10MB max)
            const int maxFileSize = 10 * 1024 * 1024;
            if (file.Length > maxFileSize)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse("File size exceeds maximum limit of 10MB."));
            }

            var command = new UploadEventImageCommand
            {
                EventId = id,
                FileStream = file.OpenReadStream(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileSize = file.Length,
                IsPrimary = isPrimary,
            };

            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Created(
                $"/api/events/{id}/images/{result.Value}",
                ApiResponse<int>.SuccessResponse(result.Value, "Image uploaded successfully"));
        }

        private static async Task<IResult> SetPrimaryImageAsync(
            int eventId,
            int imageId,
            ISender mediator)
        {
            var command = new SetPrimaryImageCommand
            {
                EventId = eventId,
                ImageId = imageId,
            };

            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse.SuccessResponse("Primary image set successfully"));
        }

        private static async Task<IResult> DeleteEventImageAsync(
            int eventId,
            int imageId,
            ISender mediator)
        {
            var command = new DeleteEventImageCommand
            {
                EventId = eventId,
                ImageId = imageId,
            };

            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse.SuccessResponse("Image deleted successfully"));
        }
    }
}