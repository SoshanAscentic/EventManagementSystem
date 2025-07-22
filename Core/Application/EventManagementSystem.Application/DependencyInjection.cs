// <copyright file="DependencyInjection.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application
{
    using System.Reflection;
    using EventManagementSystem.Application.Common.Behaviors;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.Common.Services;
    using FluentValidation;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // Register MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

            // Register AutoMapper
            services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

            // Register FluentValidation
            services.AddValidatorsFromAssembly(assembly);

            // Register MediatR behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

            // Register custom application services
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<INotificationService, NotificationService>();

            return services;
        }
    }
}
