// <copyright file="UpdateUserProfileCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UpdateUserProfile
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result>
    {
        private readonly IUserRepository userRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<UpdateUserProfileCommandHandler> logger;

        public UpdateUserProfileCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateUserProfileCommandHandler> logger)
        {
            this.userRepository = userRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Updating user profile: {UserId}", request.UserId);

                var user = await this.userRepository.GetByIdAsync(UserId.Create(request.UserId), cancellationToken);
                if (user == null)
                {
                    this.logger.LogWarning("User not found: {UserId}", request.UserId);
                    return DomainErrors.User.NotFound(request.UserId);
                }

                user.UpdateProfile(request.FirstName, request.LastName, request.Phone);

                this.userRepository.Update(user);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation("Successfully updated user profile: {UserId}", request.UserId);
                return Result.Success();
            }
            catch (ArgumentException ex) when (ex.Message.Contains("FirstName") && ex.Message.Contains("empty"))
            {
                this.logger.LogWarning(ex, "First name cannot be empty");
                return DomainErrors.User.FirstNameEmpty();
            }
            catch (ArgumentException ex) when (ex.Message.Contains("LastName") && ex.Message.Contains("empty"))
            {
                this.logger.LogWarning(ex, "Last name cannot be empty");
                return DomainErrors.User.LastNameEmpty();
            }
            catch (ArgumentException ex) when (ex.Message.Contains("characters"))
            {
                this.logger.LogWarning(ex, "Name too long");
                return DomainErrors.User.NameTooLong(50);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("phone"))
            {
                this.logger.LogWarning(ex, "Invalid phone format: {Phone}", request.Phone);
                return DomainErrors.User.InvalidPhone(request.Phone ?? string.Empty);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID"))
            {
                this.logger.LogWarning(ex, "Invalid user ID provided: {UserId}", request.UserId);
                return DomainErrors.General.InvalidId("User");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error updating user profile: {UserId}", request.UserId);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
