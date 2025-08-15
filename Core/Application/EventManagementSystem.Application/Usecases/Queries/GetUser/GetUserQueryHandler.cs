// <copyright file="GetUserQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetUser
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly ILogger<GetUserQueryHandler> logger;

        public GetUserQueryHandler(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<GetUserQueryHandler> logger)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Getting user: {UserId}", request.Id);

                var user = await this.userRepository.GetByIdAsync(UserId.Create(request.Id), cancellationToken);
                if (user == null)
                {
                    this.logger.LogWarning("User not found: {UserId}", request.Id);
                    return DomainErrors.User.NotFound(request.Id);
                }

                var userDto = this.mapper.Map<UserDto>(user);
                this.logger.LogInformation("Successfully retrieved user: {UserId}", request.Id);
                return userDto;
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID"))
            {
                this.logger.LogWarning(ex, "Invalid user ID provided: {UserId}", request.Id);
                return DomainErrors.General.InvalidId("User");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error getting user: {UserId}", request.Id);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
