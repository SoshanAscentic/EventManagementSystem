// <copyright file="SearchUsersQuery.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.SearchUsers
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;

    public class SearchUsersQuery : IRequest<Result<PagedResult<UserDto>>>
    {
        public string? SearchTerm { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
