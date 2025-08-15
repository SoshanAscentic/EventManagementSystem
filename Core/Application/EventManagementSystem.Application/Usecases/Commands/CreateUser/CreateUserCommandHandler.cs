// <copyright file="CreateUserCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CreateUser
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<int>>
    {
        private readonly IUserRepository userRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<CreateUserCommandHandler> logger;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateUserCommandHandler> logger)
        {
            this.userRepository = userRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Creating user with email: {Email}", request.Email);

                // Check if user already exists
                var email = Email.Create(request.Email);
                if (await this.userRepository.ExistsByEmailAsync(email, cancellationToken))
                {
                    this.logger.LogWarning("User with email already exists: {Email}", request.Email);
                    return DomainErrors.User.EmailAlreadyExists(request.Email);
                }

                var user = User.Create(request.Email, request.FirstName, request.LastName, request.Phone);

                await this.userRepository.AddAsync(user, cancellationToken);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation("Successfully created user with ID: {UserId}", user.Id);
                return user.Id;
            }
            catch (ArgumentException ex) when (ex.Message.Contains("email"))
            {
                this.logger.LogWarning(ex, "Invalid email format: {Email}", request.Email);
                return DomainErrors.User.InvalidEmail(request.Email);
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
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error creating user: {Email}", request.Email);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
