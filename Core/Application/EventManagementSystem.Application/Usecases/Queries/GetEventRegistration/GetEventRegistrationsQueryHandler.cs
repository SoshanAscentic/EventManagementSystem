// <copyright file="GetEventRegistrationsQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetEventRegistration
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetEventRegistrationsQueryHandler : IRequestHandler<GetEventRegistrationsQuery, Result<PagedResult<RegistrationDto>>>
    {
        private readonly IEventRegistrationRepository registrationRepository;
        private readonly IMapper mapper;
        private readonly ILogger<GetEventRegistrationsQueryHandler> logger;

        public GetEventRegistrationsQueryHandler(
            IEventRegistrationRepository registrationRepository,
            IMapper mapper,
            ILogger<GetEventRegistrationsQueryHandler> logger)
        {
            this.registrationRepository = registrationRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<PagedResult<RegistrationDto>>> Handle(GetEventRegistrationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Getting registrations for event: {EventId}, Status: {Status}", request.EventId, request.Status);

                var eventId = Domain.ValueObjects.EventId.Create(request.EventId);

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

                this.logger.LogInformation(
                    "Successfully retrieved {Count} registrations out of {Total} for event {EventId}",
                    registrations.Count,
                    totalCount,
                    request.EventId);
                return pagedResult;
            }
            catch (ArgumentException ex) when (ex.Message.Contains("status"))
            {
                this.logger.LogWarning(ex, "Invalid registration status provided: {Status}", request.Status);
                return DomainErrors.Registration.InvalidStatus(request.Status ?? string.Empty);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID"))
            {
                this.logger.LogWarning(ex, "Invalid event ID provided: {EventId}", request.EventId);
                return DomainErrors.General.InvalidId("Event");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error getting event registrations for event: {EventId}", request.EventId);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
