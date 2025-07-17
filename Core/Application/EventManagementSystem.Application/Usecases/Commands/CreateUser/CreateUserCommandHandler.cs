// <copyright file="CreateUserCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CreateUser
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<int>>
    {
        private readonly IUserRepository userRepository;
        private readonly IUnitOfWork unitOfWork;

        public CreateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            this.userRepository = userRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            // Check if user already exists
            var email = Email.Create(request.Email);
            if (await userRepository.ExistsByEmailAsync(email, cancellationToken))
            {
                return Result.Failure<int>("User with this email already exists");
            }

            try
            {
                var user = User.Create(request.Email, request.FirstName, request.LastName, request.Phone);

                await userRepository.AddAsync(user, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success(user.Id);
            }
            catch (Exception ex)
            {
                return Result.Failure<int>($"Failed to create user: {ex.Message}");
            }
        }
    }
}
