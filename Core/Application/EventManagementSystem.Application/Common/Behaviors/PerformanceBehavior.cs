// <copyright file="PerformanceBehavior.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Behaviors
{
    using System.Diagnostics;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> logger;
        private readonly TimeSpan slowQueryThreshold = TimeSpan.FromSeconds(5);

        public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
        {
            this.logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            var response = await next();

            stopwatch.Stop();

            if (stopwatch.Elapsed > this.slowQueryThreshold)
            {
                var requestName = typeof(TRequest).Name;
                this.logger.LogWarning(
                    "Slow query detected: {RequestName} took {ElapsedMs}ms {@Request}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    request);
            }

            return response;
        }
    }
}
