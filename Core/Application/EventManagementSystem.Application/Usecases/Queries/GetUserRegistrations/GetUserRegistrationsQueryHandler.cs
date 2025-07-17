// <copyright file="GetUserRegistrationsQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetUserRegistrations
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class GetUserRegistrationsQueryHandler : IRequestHandler<GetUserRegistrationsQuery, Result<PagedResult<RegistrationDto>>>
    {
        private readonly IEventRegistrationRepository registrationRepository;
        private readonly IMapper mapper;

        public GetUserRegistrationsQueryHandler(IEventRegistrationRepository registrationRepository, IMapper mapper)
        {
            this.registrationRepository = registrationRepository;
            this.mapper = mapper;
        }

        public async Task<Result<PagedResult<RegistrationDto>>> Handle(GetUserRegistrationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
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

                return Result.Success(pagedResult);
            }
            catch (Exception ex)
            {
                return Result.Failure<PagedResult<RegistrationDto>>($"Failed to retrieve user registrations: {ex.Message}");
            }
        }
    }
}
