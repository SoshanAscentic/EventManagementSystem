// <copyright file="IStatisticsService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Interfaces
{
    using EventManagementSystem.Application.DTOs;

    public interface IStatisticsService
    {
        Task<EventStatisticsDto> GetEventStatisticsAsync(DateTime? fromDate = null, CancellationToken cancellationToken = default);

        Task<List<EventSummaryDto>> GetEventSummariesAsync(
            int? categoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool upcomingOnly = false,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<List<RegistrationSummaryDto>> GetRegistrationSummariesAsync(
            int? eventId = null,
            int? userId = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default);

        Task<Dictionary<string, object>> GetDashboardDataAsync(CancellationToken cancellationToken = default);

        Task<List<Domain.Entities.Event>> GetEventsNearingCapacityAsync(
            double thresholdPercentage = 0.8,
            CancellationToken cancellationToken = default);
    }
}
