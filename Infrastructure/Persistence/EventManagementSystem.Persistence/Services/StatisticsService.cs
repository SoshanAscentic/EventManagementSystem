// <copyright file="StatisticsService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Services
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Persistence.Context;
    using EventManagementSystem.Persistence.Extensions;
    using Microsoft.Extensions.Logging;

    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<StatisticsService> logger;

        public StatisticsService(ApplicationDbContext context, ILogger<StatisticsService> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<EventStatisticsDto> GetEventStatisticsAsync(DateTime? fromDate = null, CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Getting event statistics from date: {FromDate}", fromDate);

                var statistics = await this.context.GetEventStatisticsAsync(fromDate, cancellationToken);

                this.logger.LogInformation("Successfully retrieved event statistics");
                return statistics;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting event statistics");
                throw;
            }
        }

        public async Task<List<EventSummaryDto>> GetEventSummariesAsync(
            int? categoryId = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            bool upcomingOnly = false,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Getting event summaries with filters");

                var summaries = await this.context.GetEventSummariesAsync(
                    categoryId, startDate, endDate, upcomingOnly, pageNumber, pageSize, cancellationToken);

                this.logger.LogInformation("Successfully retrieved {Count} event summaries", summaries.Count);
                return summaries;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting event summaries");
                throw;
            }
        }

        public async Task<List<RegistrationSummaryDto>> GetRegistrationSummariesAsync(
            int? eventId = null,
            int? userId = null,
            string? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Getting registration summaries with filters");

                var summaries = await this.context.GetRegistrationSummariesAsync(
                    eventId, userId, status, fromDate, toDate, pageNumber, pageSize, cancellationToken);

                this.logger.LogInformation("Successfully retrieved {Count} registration summaries", summaries.Count);
                return summaries;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting registration summaries");
                throw;
            }
        }

        public async Task<Dictionary<string, object>> GetDashboardDataAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Getting dashboard data");

                var data = await this.context.GetDashboardDataAsync(cancellationToken);

                this.logger.LogInformation("Successfully retrieved dashboard data");
                return data;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting dashboard data");
                throw;
            }
        }

        public async Task<List<Event>> GetEventsNearingCapacityAsync(
            double thresholdPercentage = 0.8,
            CancellationToken cancellationToken = default)
        {
            try
            {
                this.logger.LogInformation("Getting events nearing capacity with threshold: {Threshold}", thresholdPercentage);

                var events = await this.context.GetEventsNearingCapacityAsync(thresholdPercentage, cancellationToken);

                this.logger.LogInformation("Successfully retrieved {Count} events nearing capacity", events.Count);
                return events;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting events nearing capacity");
                throw;
            }
        }
    }
}
