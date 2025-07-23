// <copyright file="AdminEndpoints.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Endpoints
{
    using EventManagementSystem.API.Models;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Application.Usecases.Queries.GetEventStatistics;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public static class AdminEndpoints
    {
        public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
        {
            var admin = app.MapGroup("/api/admin")
                .WithTags("Administration")
                .RequireAuthorization("RequireAdminRole")
                .RequireRateLimiting("Api");

            // Dashboard and statistics
            admin.MapGet("/dashboard", GetDashboardDataAsync)
                .WithName("GetDashboardData")
                .WithSummary("Get admin dashboard data")
                .WithDescription("Retrieves comprehensive dashboard data for administrators")
                .Produces<ApiResponse<Dictionary<string, object>>>(200)
                .Produces<ApiResponse>(403);

            admin.MapGet("/statistics", GetEventStatisticsAsync)
                .WithName("GetEventStatistics")
                .WithSummary("Get event statistics")
                .WithDescription("Retrieves detailed event and registration statistics")
                .Produces<ApiResponse<EventStatisticsDto>>(200)
                .Produces<ApiResponse>(403);

            admin.MapGet("/events/capacity-alerts", GetEventsNearingCapacityAsync)
                .WithName("GetEventsNearingCapacity")
                .WithSummary("Get events nearing capacity")
                .WithDescription("Retrieves events that are approaching full capacity")
                .Produces<ApiResponse<List<EventDto>>>(200)
                .Produces<ApiResponse>(403);
        }

        private static async Task<IResult> GetDashboardDataAsync(
            IStatisticsService statisticsService)
        {
            try
            {
                var dashboardData = await statisticsService.GetDashboardDataAsync();
                return Results.Ok(ApiResponse<Dictionary<string, object>>.SuccessResponse(dashboardData));
            }
            catch (Exception)
            {
                return Results.Problem("Error retrieving dashboard data");
            }
        }

        private static async Task<IResult> GetEventStatisticsAsync(
            [FromQuery] DateTime? fromDate,
            ISender mediator)
        {
            var query = new GetEventStatisticsQuery { FromDate = fromDate };
            var result = await mediator.Send(query);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse<EventStatisticsDto>.SuccessResponse(result.Value));
        }

        private static async Task<IResult> GetEventsNearingCapacityAsync(
            IStatisticsService statisticsService,
            [FromQuery] double threshold = 0.8)
        {
            try
            {
                var events = await statisticsService.GetEventsNearingCapacityAsync(threshold);

                // Map to DTOs if you have AutoMapper configured, or create a simple mapping
                var eventDtos = events.Select(e => new EventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartDateTime = e.EventDateTime.StartDateTime,
                    Venue = e.Location.Venue,
                    Capacity = e.Capacity.Value,
                    CurrentRegistrations = e.CurrentRegistrations,
                    CategoryName = e.Category?.Name ?? "Unknown",
                    EventType = e.EventType.Value,
                    IsRegistrationOpen = e.IsRegistrationOpen,
                }).ToList();

                return Results.Ok(ApiResponse<List<EventDto>>.SuccessResponse(
                    eventDtos,
                    $"Found {eventDtos.Count} events at {threshold * 100}% capacity or higher"));
            }
            catch (Exception)
            {
                return Results.Problem("Error retrieving events nearing capacity");
            }
        }
    }
}
