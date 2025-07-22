// <copyright file="SearchUsersQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.SearchUsers
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, Result<PagedResult<UserDto>>>
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly ILogger<SearchUsersQueryHandler> logger;

        public SearchUsersQueryHandler(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<SearchUsersQueryHandler> logger)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<PagedResult<UserDto>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation(
                    "Searching users with term: {SearchTerm}, Page: {Page}, Size: {Size}",
                    request.SearchTerm,
                    request.PageNumber,
                    request.PageSize);

                var (users, totalCount) = await this.userRepository.SearchUsersAsync(
                    request.SearchTerm,
                    request.PageNumber,
                    request.PageSize,
                    cancellationToken);

                var userDtos = this.mapper.Map<List<UserDto>>(users);
                var pagedResult = new PagedResult<UserDto>(userDtos, totalCount, request.PageNumber, request.PageSize);

                this.logger.LogInformation(
                    "Successfully found {Count} users out of {Total}",
                    users.Count,
                    totalCount);

                return pagedResult;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error searching users");
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
