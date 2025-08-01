// <copyright file="AuthenticationService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Identity.Services
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Identity.Context;
    using EventManagementSystem.Identity.Entities;
    using EventManagementSystem.Identity.Interfaces;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IJwtService jwtService;
        private readonly IdentityDbContext identityContext;
        private readonly IUserRepository userRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<AuthenticationService> logger;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            IJwtService jwtService,
            IdentityDbContext identityContext,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ILogger<AuthenticationService> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.jwtService = jwtService;
            this.identityContext = identityContext;
            this.userRepository = userRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request, string ipAddress)
        {
            try
            {
                this.logger.LogInformation("Login attempt for email: {Email}", request.Email);

                var user = await this.userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    this.logger.LogWarning("Login failed - user not found: {Email}", request.Email);
                    return DomainErrors.Authentication.InvalidCredentials();
                }

                if (!user.IsActive)
                {
                    this.logger.LogWarning("Login failed - user account deactivated: {Email}", request.Email);
                    return DomainErrors.User.AccountDeactivated(user.Id);
                }

                var result = await this.signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    this.logger.LogWarning(
                        "Login failed for user: {Email}. Reason: {Reason}",
                        request.Email,
                        result.IsLockedOut ? "Account locked" :
                        result.IsNotAllowed ? "Not allowed" : "Invalid credentials");

                    if (result.IsLockedOut)
                    {
                        return DomainErrors.Authentication.AccountLocked();
                    }

                    return DomainErrors.Authentication.InvalidCredentials();
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await this.userManager.UpdateAsync(user);

                var response = await this.GenerateAuthResponseAsync(user, ipAddress);

                this.logger.LogInformation("User logged in successfully: {Email}", request.Email);
                return response;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during login for email: {Email}", request.Email);
                return DomainErrors.General.UnexpectedError();
            }
        }

        public async Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request, string ipAddress)
        {
            try
            {
                this.logger.LogInformation("Registration attempt for email: {Email}", request.Email);

                // Check if user already exists
                if (await this.userManager.FindByEmailAsync(request.Email) != null)
                {
                    this.logger.LogWarning("Registration failed - email already exists: {Email}", request.Email);
                    return DomainErrors.User.EmailAlreadyExists(request.Email);
                }

                // Create Identity user
                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Phone = request.Phone,
                    EmailConfirmed = false, // Require email confirmation
                };

                var result = await this.userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    this.logger.LogWarning(
                        "Registration failed for email: {Email}. Errors: {Errors}",
                        request.Email,
                        string.Join(", ", errors));
                    return Result<AuthenticationResponse>.ValidationFailure(errors);
                }

                // Assign default role
                await this.userManager.AddToRoleAsync(user, "User");

                // Create corresponding domain user
                var domainUser = Domain.Entities.User.Create(
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.Phone);

                await this.userRepository.AddAsync(domainUser);
                await this.unitOfWork.SaveChangesAsync();

                // Link Identity user to domain user
                user.DomainUserId = domainUser.Id;
                await this.userManager.UpdateAsync(user);

                // Generate email confirmation token
                var emailToken = await this.userManager.GenerateEmailConfirmationTokenAsync(user);

                var response = await this.GenerateAuthResponseAsync(user, ipAddress);

                this.logger.LogInformation("User registered successfully: {Email}", request.Email);
                return response;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during registration for email: {Email}", request.Email);
                return DomainErrors.General.UnexpectedError();
            }
        }

        public async Task<Result<AuthenticationResponse>> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            try
            {
                var token = await this.identityContext.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (token == null || !token.IsActive)
                {
                    this.logger.LogWarning("Invalid refresh token attempted from IP: {IpAddress}", ipAddress);
                    return DomainErrors.Authentication.TokenInvalid();
                }

                // Replace old refresh token with new one
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = ipAddress;

                var newRefreshToken = this.jwtService.GenerateRefreshToken(ipAddress);
                newRefreshToken.UserId = token.UserId;
                token.ReplacedByToken = newRefreshToken.Token;

                this.identityContext.RefreshTokens.Add(newRefreshToken);
                await this.identityContext.SaveChangesAsync();

                var response = await this.GenerateAuthResponseAsync(token.User, ipAddress);

                this.logger.LogInformation("Token refreshed for user: {UserId}", token.UserId);
                return response;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during token refresh from IP: {IpAddress}", ipAddress);
                return DomainErrors.Authentication.TokenInvalid();
            }
        }

        public async Task<Result> LogoutAsync(string refreshToken, string ipAddress)
        {
            try
            {
                var token = await this.identityContext.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                if (token != null && token.IsActive)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = ipAddress;
                    await this.identityContext.SaveChangesAsync();
                }

                this.logger.LogInformation("User logged out from IP: {IpAddress}", ipAddress);
                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during logout from IP: {IpAddress}", ipAddress);
                return DomainErrors.General.UnexpectedError();
            }
        }

        public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await this.userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return DomainErrors.User.NotFound(userId);
                }

                var result = await this.userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return Result.ValidationFailure(errors);
                }

                this.logger.LogInformation("Password changed for user: {UserId}", userId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                return DomainErrors.General.UnexpectedError();
            }
        }

        public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            try
            {
                var user = await this.userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    // Return success even if user doesn't exist for security
                    return Result.Success();
                }

                var token = await this.userManager.GeneratePasswordResetTokenAsync(user);

                this.logger.LogInformation("Password reset requested for email: {Email}", request.Email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error processing forgot password for email: {Email}", request.Email);
                return DomainErrors.General.UnexpectedError();
            }
        }

        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                var user = await this.userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return DomainErrors.User.NotFoundByEmail(request.Email);
                }

                var result = await this.userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return Result.ValidationFailure(errors);
                }

                this.logger.LogInformation("Password reset successfully for email: {Email}", request.Email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error resetting password for email: {Email}", request.Email);
                return DomainErrors.General.UnexpectedError();
            }
        }

        public async Task<Result> ConfirmEmailAsync(int userId, string token)
        {
            try
            {
                var user = await this.userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return DomainErrors.User.NotFound(userId);
                }

                var result = await this.userManager.ConfirmEmailAsync(user, token);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return Result.ValidationFailure(errors);
                }

                this.logger.LogInformation("Email confirmed for user: {UserId}", userId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error confirming email for user: {UserId}", userId);
                return DomainErrors.General.UnexpectedError();
            }
        }

        public async Task<Result> ResendEmailConfirmationAsync(string email)
        {
            try
            {
                var user = await this.userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Return success even if user doesn't exist for security
                    return Result.Success();
                }

                if (user.EmailConfirmed)
                {
                    return Result.Success(); // Already confirmed
                }

                var token = await this.userManager.GenerateEmailConfirmationTokenAsync(user);

                this.logger.LogInformation("Email confirmation resent for email: {Email}", email);
                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error resending email confirmation for email: {Email}", email);
                return DomainErrors.General.UnexpectedError();
            }
        }

        public async Task<AuthenticationResponse?> GetCurrentUserAsync(int userId)
        {
            try
            {
                var user = await this.userManager.FindByIdAsync(userId.ToString());
                if (user == null || !user.IsActive)
                {
                    return null;
                }

                var roles = await this.userManager.GetRolesAsync(user);

                return new AuthenticationResponse
                {
                    UserId = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Roles = roles,
                    IsEmailConfirmed = user.EmailConfirmed,
                    // Don't include AccessToken or RefreshToken for security
                };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting current user: {UserId}", userId);
                return null;
            }
        }

        public async Task<Result> RevokeTokenAsync(string token, string ipAddress)
        {
            try
            {
                var refreshToken = await this.identityContext.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == token);

                if (refreshToken == null || !refreshToken.IsActive)
                {
                    return DomainErrors.Authentication.TokenInvalid();
                }

                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.RevokedByIp = ipAddress;
                await this.identityContext.SaveChangesAsync();

                this.logger.LogInformation(
                    "Token revoked for user: {UserId} from IP: {IpAddress}",
                    refreshToken.UserId,
                    ipAddress);
                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error revoking token from IP: {IpAddress}", ipAddress);
                return DomainErrors.General.UnexpectedError();
            }
        }

        public async Task<Result> RevokeAllUserTokensAsync(int userId, string ipAddress)
        {
            try
            {
                var refreshTokens = await this.identityContext.RefreshTokens
                    .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
                    .ToListAsync();

                foreach (var token in refreshTokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = ipAddress;
                }

                await this.identityContext.SaveChangesAsync();

                this.logger.LogInformation(
                    "All tokens revoked for user: {UserId} from IP: {IpAddress}",
                    userId,
                    ipAddress);
                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error revoking all tokens for user: {UserId}", userId);
                return DomainErrors.General.UnexpectedError();
            }
        }

        private async Task<AuthenticationResponse> GenerateAuthResponseAsync(ApplicationUser user, string ipAddress)
        {
            var roles = await this.userManager.GetRolesAsync(user);
            var accessToken = this.jwtService.GenerateAccessToken(user, roles);
            var refreshToken = this.jwtService.GenerateRefreshToken(ipAddress);

            // Save refresh token
            refreshToken.UserId = user.Id;
            this.identityContext.RefreshTokens.Add(refreshToken);
            await this.identityContext.SaveChangesAsync();

            return new AuthenticationResponse
            {
                UserId = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Roles = roles,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token, // ADD THIS LINE
                ExpiresAt = DateTime.UtcNow.AddMinutes(15), // JWT expiry
                IsEmailConfirmed = user.EmailConfirmed,
            };
        }
    }
}
