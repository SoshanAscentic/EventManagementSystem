// <copyright file="CategoryEndpoints.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Endpoints
{
    using EventManagementSystem.API.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Application.Usecases.Commands.CreateCategory;
    using EventManagementSystem.Application.Usecases.Queries.GetCategory;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var categories = app.MapGroup("/api/categories")
                .WithTags("Categories")
                .RequireRateLimiting("Api");

            // Public endpoints
            categories.MapGet("/", GetCategoriesAsync)
                .WithName("GetCategories")
                .WithSummary("Get all event categories")
                .WithDescription("Retrieves all active event categories")
                .Produces<ApiResponse<List<CategoryDto>>>(200);

            // Admin only endpoints
            categories.MapPost("/", CreateCategoryAsync)
                .WithName("CreateCategory")
                .WithSummary("Create a new category")
                .WithDescription("Creates a new event category (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse<int>>(201)
                .Produces<ApiResponse>(400)
                .Produces<ApiResponse>(403);
        }

        private static async Task<IResult> GetCategoriesAsync(
            ISender mediator,
            [FromQuery] bool activeOnly = true)
        {
            var query = new GetCategoriesQuery { ActiveOnly = activeOnly };
            var result = await mediator.Send(query);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(result.Value));
        }

        private static async Task<IResult> CreateCategoryAsync(
            [FromBody] CreateCategoryCommand command,
            ISender mediator)
        {
            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Created(
                $"/api/categories/{result.Value}",
                ApiResponse<int>.SuccessResponse(result.Value, "Category created successfully"));
        }
    }
}
