// <copyright file="GetCategoriesQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetCategory
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
    {
        private readonly IEventCategoryRepository categoryRepository;
        private readonly IMapper mapper;
        private readonly ILogger<GetCategoriesQueryHandler> logger;

        public GetCategoriesQueryHandler(
            IEventCategoryRepository categoryRepository,
            IMapper mapper,
            ILogger<GetCategoriesQueryHandler> logger)
        {
            this.categoryRepository = categoryRepository;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Getting categories, ActiveOnly: {ActiveOnly}", request.ActiveOnly);

                var categories = request.ActiveOnly
                    ? await this.categoryRepository.GetActiveAsync(cancellationToken)
                    : await this.categoryRepository.GetAllAsync(cancellationToken);

                var categoryDtos = this.mapper.Map<List<CategoryDto>>(categories);

                this.logger.LogInformation("Successfully retrieved {Count} categories", categoryDtos.Count);
                return categoryDtos;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error getting categories");
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
