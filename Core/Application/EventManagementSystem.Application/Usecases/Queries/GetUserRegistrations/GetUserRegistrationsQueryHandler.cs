// <copyright file="GetUserRegistrationsQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetUserRegistrations
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetUserRegistrationsQueryHandler : IRequestHandler<GetUserRegistrationsQuery, Result<PagedResult<RegistrationDto>>>
    {
        private readonly IEventRegistrationRepository registrationRepository;
        private readonly IMapper mapper;
        private readonly ILogger<GetUserRegistrationsQueryHandler> logger;

        public GetUserRegistrationsQueryHandler(
            IEventRegistrationRepository registrationRepository,
            IMapper mapper,
            ILogger<GetUserRegistrationsQueryHandler> logger)
        {
            this.registrationRepository = registrationRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<PagedResult<RegistrationDto>>> Handle(GetUserRegistrationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Getting registrations for user: {UserId}", request.UserId);

                var userId = UserId.Create(request.UserId);
                var registrations = await this.registrationRepository.GetUserRegistrationHistoryAsync(
                    userId,
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

                var totalCount = await this.registrationRepository.CountAsync(
                    r => r.UserId == userId,
                    cancellationToken);

                var registrationDtos = this.mapper.Map<List<RegistrationDto>>(registrations);
                var pagedResult = new PagedResult<RegistrationDto>(registrationDtos, totalCount, request.PageNumber, request.PageSize);

                this.logger.LogInformation(
                    "Successfully retrieved {Count} registrations out of {Total} for user {UserId}",
                    registrations.Count,
                    totalCount,
                    request.UserId);
                return pagedResult;
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID"))
            {
                this.logger.LogWarning(ex, "Invalid user ID provided: {UserId}", request.UserId);
                return DomainErrors.General.InvalidId("User");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error getting user registrations for user: {UserId}", request.UserId);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
