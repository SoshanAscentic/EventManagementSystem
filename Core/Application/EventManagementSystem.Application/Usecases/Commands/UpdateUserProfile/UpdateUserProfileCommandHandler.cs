// <copyright file="UpdateUserProfileCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UpdateUserProfile
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, Result>
    {
        private readonly IUserRepository userRepository;
        private readonly IUnitOfWork unitOfWork;

        public UpdateUserProfileCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            this.userRepository = userRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await this.userRepository.GetByIdAsync(UserId.Create(request.UserId), cancellationToken);
            if (user == null)
            {
                return Result.Failure("User not found");
            }

            try
            {
                user.UpdateProfile(request.FirstName, request.LastName, request.Phone);

                this.userRepository.Update(user);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed to update user profile: {ex.Message}");
            }
        }
    }
}
