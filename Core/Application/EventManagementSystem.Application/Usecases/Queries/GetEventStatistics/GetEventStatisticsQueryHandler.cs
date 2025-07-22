namespace EventManagementSystem.Application.Usecases.Queries.GetEventStatistics
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetEventStatisticsQueryHandler : IRequestHandler<GetEventStatisticsQuery, Result<EventStatisticsDto>>
    {
        private readonly IStatisticsService statisticsService;
        private readonly ILogger<GetEventStatisticsQueryHandler> logger;

        public GetEventStatisticsQueryHandler(
            IStatisticsService statisticsService,
            ILogger<GetEventStatisticsQueryHandler> logger)
        {
            this.statisticsService = statisticsService;
            this.logger = logger;
        }

        public async Task<Result<EventStatisticsDto>> Handle(GetEventStatisticsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Getting event statistics from date: {FromDate}", request.FromDate);

                var statistics = await this.statisticsService.GetEventStatisticsAsync(request.FromDate, cancellationToken);

                this.logger.LogInformation("Successfully retrieved event statistics");
                return statistics;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error getting event statistics");
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
