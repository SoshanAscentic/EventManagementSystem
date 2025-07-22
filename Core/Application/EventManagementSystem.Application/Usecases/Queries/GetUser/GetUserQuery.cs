// <copyright file="GetUsersQuery.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetUser
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;

    public class GetUserQuery : IRequest<Result<UserDto>>
    {
        public GetUserQuery(int id)
        {
            this.Id = id;
        }

        public int Id { get; set; }
    }
}
