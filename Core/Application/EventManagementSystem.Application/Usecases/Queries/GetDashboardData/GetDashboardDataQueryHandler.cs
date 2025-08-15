// <copyright file="GetDashboardDataQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetDashboardData
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.Common.Models;
    using MediatR;

    public class GetDashboardDataQueryHandler : IRequestHandler<GetDashboardDataQuery, Result<Dictionary<string, object>>>
    {
        private readonly IStatisticsService? statisticsService;

        public async Task<Result<Dictionary<string, object>>> Handle(GetDashboardDataQuery request, CancellationToken cancellationToken)
        {
            var data = await this.statisticsService.GetDashboardDataAsync(cancellationToken);
            return Result<Dictionary<string, object>>.Success(data);
        }
    }
}
