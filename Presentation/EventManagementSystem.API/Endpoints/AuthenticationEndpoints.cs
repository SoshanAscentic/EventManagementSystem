// <copyright file="AuthenticationEndpoints.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.API.Endpoints
{
    using EventManagementSystem.API.Models;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.DTOs;
    using Microsoft.AspNetCore.Mvc;

    public static class AuthenticationEndpoints
    {
        public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder app)
        {
            var auth = app.MapGroup("/api/auth")
                .WithTags("Authentication")
                .RequireRateLimiting("Auth");

            auth.MapPost("/login", LoginAsync)
                .WithName("Login")
                .WithSummary("User login with HTTP-only cookies")
                .WithDescription("Authenticates a user and sets HTTP-only cookies for secure token management")
                .Produces<ApiResponse<AuthenticationResponse>>(200)
                .Produces<ApiResponse>(400)
                .Produces<ApiResponse>(401);

            auth.MapPost("/register", RegisterAsync)
                .WithName("Register")
                .WithSummary("User registration")
                .WithDescription("Registers a new user account")
                .Produces<ApiResponse<AuthenticationResponse>>(201)
                .Produces<ApiResponse>(400);

            auth.MapPost("/refresh", RefreshTokenAsync)
                .WithName("RefreshToken")
                .WithSummary("Refresh access token")
                .WithDescription("Refreshes the access token using the refresh token from HTTP-only cookie")
                .Produces<ApiResponse<AuthenticationResponse>>(200)
                .Produces<ApiResponse>(401);

            auth.MapPost("/logout", LogoutAsync)
                .WithName("Logout")
                .WithSummary("User logout")
                .WithDescription("Logs out the user and clears HTTP-only cookies")
                .Produces<ApiResponse>(200);

            auth.MapGet("/me", GetCurrentUserAsync)
                .WithName("GetCurrentUser")
                .WithSummary("Get current user information")
                .WithDescription("Returns information about the currently authenticated user")
                .RequireAuthorization()
                .Produces<ApiResponse<AuthenticationResponse>>(200)
                .Produces<ApiResponse>(401);

            auth.MapPost("/change-password", ChangePasswordAsync)
                .WithName("ChangePassword")
                .WithSummary("Change user password")
                .WithDescription("Changes the password for the currently authenticated user")
                .RequireAuthorization()
                .Produces<ApiResponse>(200)
                .Produces<ApiResponse>(400);
        }

        private static async Task<IResult> LoginAsync(
            [FromBody] LoginRequest request,
            IAuthenticationService authService,
            HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await authService.LoginAsync(request, ipAddress);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            // Set HTTP-only cookies
            SetAuthenticationCookies(context, result.Value);

            return Results.Ok(ApiResponse<AuthenticationResponse>.SuccessResponse(
                result.Value,
                "Login successful"));
        }

        private static async Task<IResult> RegisterAsync(
            [FromBody] RegisterRequest request,
            IAuthenticationService authService,
            HttpContext context)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await authService.RegisterAsync(request, ipAddress);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            // Set HTTP-only cookies
            SetAuthenticationCookies(context, result.Value);

            return Results.Created("/api/auth/me", ApiResponse<AuthenticationResponse>.SuccessResponse(
                result.Value,
                "Registration successful"));
        }

        private static async Task<IResult> RefreshTokenAsync(
            IAuthenticationService authService,
            HttpContext context)
        {
            var refreshToken = context.Request.Cookies["RefreshToken"];

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Results.Unauthorized();
            }

            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await authService.RefreshTokenAsync(refreshToken, ipAddress);

            if (result.IsFailure)
            {
                ClearAuthenticationCookies(context);
                return Results.Unauthorized();
            }

            // Set new HTTP-only cookies
            SetAuthenticationCookies(context, result.Value);

            return Results.Ok(ApiResponse<AuthenticationResponse>.SuccessResponse(
                result.Value,
                "Token refreshed successfully"));
        }

        private static async Task<IResult> LogoutAsync(
            IAuthenticationService authService,
            HttpContext context)
        {
            var refreshToken = context.Request.Cookies["RefreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                await authService.LogoutAsync(refreshToken, ipAddress);
            }

            ClearAuthenticationCookies(context);

            return Results.Ok(ApiResponse.SuccessResponse("Logout successful"));
        }

        private static async Task<IResult> GetCurrentUserAsync(
            IAuthenticationService authService,
            HttpContext context)
        {
            var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("id");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var user = await authService.GetCurrentUserAsync(userId);

            if (user == null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(ApiResponse<AuthenticationResponse>.SuccessResponse(user));
        }

        private static async Task<IResult> ChangePasswordAsync(
            [FromBody] ChangePasswordRequest request,
            IAuthenticationService authService,
            HttpContext context)
        {
            var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("id");

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Results.Unauthorized();
            }

            var result = await authService.ChangePasswordAsync(userId, request);

            if (result.IsFailure)
            {
                return Results.BadRequest(ApiResponse.ErrorResponse(result.GetErrorMessages().ToList()));
            }

            return Results.Ok(ApiResponse.SuccessResponse("Password changed successfully"));
        }

        private static void SetAuthenticationCookies(HttpContext context, AuthenticationResponse authResponse)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = authResponse.ExpiresAt,
            };

            var refreshCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7), // Refresh token expires in 7 days
            };

            context.Response.Cookies.Append("AccessToken", authResponse.AccessToken, cookieOptions);

            // Note: RefreshToken would be set if it was returned from the service
        }

        private static void ClearAuthenticationCookies(HttpContext context)
        {
            var expiredCookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(-1),
            };

            context.Response.Cookies.Append("AccessToken", string.Empty, expiredCookieOptions);
            context.Response.Cookies.Append("RefreshToken", string.Empty, expiredCookieOptions);
        }
    }
}
