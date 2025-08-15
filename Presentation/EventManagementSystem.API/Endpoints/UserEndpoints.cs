// <copyright file="UserEndpoints.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Endpoints
{
    using EventManagementSystem.API.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Application.Usecases.Commands.UpdateUserProfile;
    using EventManagementSystem.Application.Usecases.Queries.GetUser;
    using EventManagementSystem.Application.Usecases.Queries.SearchUsers;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var users = app.MapGroup("/api/users")
                .WithTags("Users")
                .RequireRateLimiting("Api");

            // User profile endpoints
            users.MapGet("/profile", GetCurrentUserProfileAsync)
                .WithName("GetCurrentUserProfile")
                .WithSummary("Get current user profile")
                .WithDescription("Retrieves the current user's profile information")
                .RequireAuthorization()
                .Produces<ApiResponse<UserDto>>(200)
                .Produces<ApiResponse>(401);

            users.MapPut("/profile", UpdateUserProfileAsync)
                .WithName("UpdateUserProfile")
                .WithSummary("Update user profile")
                .WithDescription("Updates the current user's profile information")
                .RequireAuthorization()
                .Produces<ApiResponse>(200)
                .Produces<ApiResponse>(400)
                .Produces<ApiResponse>(401);

            // Admin endpoints
            users.MapGet("/", SearchUsersAsync)
                .WithName("SearchUsers")
                .WithSummary("Search users")
                .WithDescription("Search and retrieve users (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse<PagedResponse<UserDto>>>(200)
                .Produces<ApiResponse>(403);

            users.MapGet("/{userId:int}", GetUserByIdAsync)
                .WithName("GetUserById")
                .WithSummary("Get user by ID")
                .WithDescription("Retrieves a specific user by ID (Admin only)")
                .RequireAuthorization("RequireAdminRole")
                .Produces<ApiResponse<UserDto>>(200)
                .Produces<ApiResponse>(404)
                .Produces<ApiResponse>(403);
        }

        private static async Task<IResult> GetCurrentUserProfileAsync(
            ISender mediator,
            HttpContext context)
        {
            var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var query = new GetUserQuery(userId);
            var result = await mediator.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse<UserDto>.SuccessResponse(result.Value));
        }

        private static async Task<IResult> UpdateUserProfileAsync(
            [FromBody] UpdateUserProfileRequest request,
            ISender mediator,
            HttpContext context)
        {
            var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("id");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var command = new UpdateUserProfileCommand
            {
                UserId = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
            };

            var result = await mediator.Send(command);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse.SuccessResponse("Profile updated successfully"));
        }

        private static async Task<IResult> SearchUsersAsync(
            [AsParameters] PaginationParameters pagination,
            ISender mediator)
        {
            var query = new SearchUsersQuery
            {
                SearchTerm = pagination.SearchTerm,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
            };

            var result = await mediator.Send(query);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            var response = new PagedResponse<UserDto>
            {
                Items = result.Value.Items,
                TotalCount = result.Value.TotalCount,
                PageNumber = result.Value.PageNumber,
                PageSize = result.Value.PageSize,
                TotalPages = result.Value.TotalPages,
                HasNextPage = result.Value.HasNextPage,
                HasPreviousPage = result.Value.HasPreviousPage,
            };

            return Results.Ok(ApiResponse<PagedResponse<UserDto>>.SuccessResponse(response));
        }

        private static async Task<IResult> GetUserByIdAsync(
            int userId,
            ISender mediator)
        {
            var query = new GetUserQuery(userId);
            var result = await mediator.Send(query);

            if (result.IsFailure)
            {
                return Results.NotFound(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse<UserDto>.SuccessResponse(result.Value));
        }
    }
}
