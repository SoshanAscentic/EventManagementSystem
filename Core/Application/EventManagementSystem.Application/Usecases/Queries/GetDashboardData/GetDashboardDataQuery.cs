// <copyright file="GetDashboardDataQuery.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetDashboardData
{
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class GetDashboardDataQuery : IRequest<Result<Dictionary<string, object>>>
    {
    }
}
