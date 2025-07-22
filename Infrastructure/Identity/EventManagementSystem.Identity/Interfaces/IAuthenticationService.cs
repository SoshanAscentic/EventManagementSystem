// <copyright file="IAuthenticationService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Identity.Interfaces
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Identity.Models;

    public interface IAuthenticationService
    {
        Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request, string ipAddress);

        Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request, string ipAddress);

        Task<Result<AuthenticationResponse>> RefreshTokenAsync(string refreshToken, string ipAddress);

        Task<Result> LogoutAsync(string refreshToken, string ipAddress);

        Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request);

        Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request);

        Task<Result> ResetPasswordAsync(ResetPasswordRequest request);

        Task<Result> ConfirmEmailAsync(int userId, string token);

        Task<Result> ResendEmailConfirmationAsync(string email);

        Task<AuthenticationResponse?> GetCurrentUserAsync(int userId);

        Task<Result> RevokeTokenAsync(string token, string ipAddress);

        Task<Result> RevokeAllUserTokensAsync(int userId, string ipAddress);
    }
}
