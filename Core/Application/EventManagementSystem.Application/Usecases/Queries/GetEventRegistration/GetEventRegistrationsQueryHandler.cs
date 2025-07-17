// <copyright file="GetEventRegistrationsQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEventRegistration
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class GetEventRegistrationsQueryHandler : IRequestHandler<GetEventRegistrationsQuery, Result<PagedResult<RegistrationDto>>>
    {
        private readonly IEventRegistrationRepository registrationRepository;
        private readonly IMapper mapper;

        public GetEventRegistrationsQueryHandler(IEventRegistrationRepository registrationRepository, IMapper mapper)
        {
            this.registrationRepository = registrationRepository;
            this.mapper = mapper;
        }

        public async Task<Result<PagedResult<RegistrationDto>>> Handle(GetEventRegistrationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var eventId = EventId.Create(request.EventId);

                RegistrationStatus? status = null;
                if (!string.IsNullOrEmpty(request.Status))
                {
                    status = RegistrationStatus.Create(request.Status);
                }

                var (registrations, totalCount) = await this.registrationRepository.SearchRegistrationsAsync(
                    eventId: eventId,
                    status: status,
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    cancellationToken: cancellationToken);

                var registrationDtos = this.mapper.Map<List<RegistrationDto>>(registrations);
                var pagedResult = new PagedResult<RegistrationDto>(registrationDtos, totalCount, request.PageNumber, request.PageSize);

                return Result.Success(pagedResult);
            }
            catch (Exception ex)
            {
                return Result.Failure<PagedResult<RegistrationDto>>($"Failed to retrieve event registrations: {ex.Message}");
            }
        }
    }
}
