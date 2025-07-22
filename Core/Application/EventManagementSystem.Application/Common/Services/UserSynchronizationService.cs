using EventManagementSystem.Application.Common.Constants;
using EventManagementSystem.Application.Common.Interfaces;
using EventManagementSystem.Application.Common.Models;
using EventManagementSystem.Domain.Entities;
using EventManagementSystem.Domain.Repositories;
using EventManagementSystem.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Application.Common.Services
{
    public class UserSynchronizationService : IUserSynchronizationService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserRepository userRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly IdentityDbContext identityContext;
        private readonly ILogger<UserSynchronizationService> logger;

        public UserSynchronizationService(
            UserManager<ApplicationUser> userManager,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            IdentityDbContext identityContext,
            ILogger<UserSynchronizationService> logger)
        {
            this.userManager = userManager;
            this.userRepository = userRepository;
            this.unitOfWork = unitOfWork;
            this.identityContext = identityContext;
            this.logger = logger;
        }

        public async Task<Result> SynchronizeUserAsync(int identityUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Synchronizing user: {IdentityUserId}", identityUserId);

                var identityUser = await this.userManager.FindByIdAsync(identityUserId.ToString());
                if (identityUser == null)
                {
                    this.logger.LogWarning("Identity user not found: {IdentityUserId}", identityUserId);
                    return DomainErrors.User.NotFound(identityUserId);
                }

                // Check if domain user already exists
                if (identityUser.DomainUserId.HasValue)
                {
                    var existingDomainUser = await this.userRepository.GetByIdAsync(
                        UserId.Create(identityUser.DomainUserId.Value),
                        cancellationToken);

                    if (existingDomainUser != null)
                    {
                        // Update existing domain user
                        return await this.UpdateDomainUserFromIdentityAsync(identityUserId, cancellationToken);
                    }
                }

                // Create new domain user
                var domainUser = User.Create(
                    identityUser.Email!,
                    identityUser.FirstName,
                    identityUser.LastName,
                    identityUser.Phone);

                await this.userRepository.AddAsync(domainUser, cancellationToken);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                // Update identity user with domain user ID
                identityUser.DomainUserId = domainUser.Id;
                identityUser.UpdatedAt = DateTime.UtcNow;
                await this.userManager.UpdateAsync(identityUser);

                this.logger.LogInformation(
                    "Successfully synchronized user. Identity: {IdentityUserId}, Domain: {DomainUserId}",
                    identityUserId,
                    domainUser.Id);

                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error synchronizing user: {IdentityUserId}", identityUserId);
                return DomainErrors.General.UnexpectedError();
            }
        }

        public async Task<Result> UpdateDomainUserFromIdentityAsync(int identityUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                var identityUser = await this.userManager.FindByIdAsync(identityUserId.ToString());
                if (identityUser?.DomainUserId == null)
                {
                    return DomainErrors.User.NotFound(identityUserId);
                }

                var domainUser = await this.userRepository.GetByIdAsync(
                    UserId.Create(identityUser.DomainUserId.Value),
                    cancellationToken);

                if (domainUser == null)
                {
                    return DomainErrors.User.NotFound(identityUser.DomainUserId.Value);
                }

                // Update domain user with identity user data
                domainUser.UpdateProfile(
                    identityUser.FirstName,
                    identityUser.LastName,
                    identityUser.Phone);

                // Update email if different
                if (domainUser.Email.Value != identityUser.Email)
                {
                    domainUser.UpdateEmail(identityUser.Email!);
                }

                this.userRepository.Update(domainUser);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation("Updated domain user: {DomainUserId}", domainUser.Id);
                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error updating domain user from identity: {IdentityUserId}", identityUserId);
                return DomainErrors.General.UnexpectedError();
            }
        }

        public async Task<Result> HandleIdentityUserUpdatedAsync(int identityUserId, CancellationToken cancellationToken = default)
        {
            return await this.UpdateDomainUserFromIdentityAsync(identityUserId, cancellationToken);
        }

        public async Task<Result> HandleIdentityUserDeletedAsync(int identityUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Find the identity user to get the domain user ID
                var identityUser = await this.identityContext.Users
                    .FirstOrDefaultAsync(u => u.Id == identityUserId, cancellationToken);

                if (identityUser?.DomainUserId != null)
                {
                    var domainUser = await this.userRepository.GetByIdAsync(
                        UserId.Create(identityUser.DomainUserId.Value),
                        cancellationToken);

                    if (domainUser != null)
                    {
                        // Check if user has active registrations
                        if (domainUser.ActiveRegistrationsCount > 0)
                        {
                            this.logger.LogWarning(
                                "Cannot delete domain user with active registrations: {DomainUserId}",
                                domainUser.Id);
                            return DomainErrors.User.AccountDeactivated(domainUser.Id);
                        }

                        this.userRepository.Remove(domainUser);
                        await this.unitOfWork.SaveChangesAsync(cancellationToken);

                        this.logger.LogInformation("Deleted domain user: {DomainUserId}", domainUser.Id);
                    }
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error handling identity user deletion: {IdentityUserId}", identityUserId);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
