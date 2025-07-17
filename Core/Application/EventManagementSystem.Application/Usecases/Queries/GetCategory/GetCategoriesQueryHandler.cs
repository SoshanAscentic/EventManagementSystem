// <copyright file="GetCategoriesQueryHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetCategory
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Repositories;
    using MediatR;

    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
    {
        private readonly IEventCategoryRepository categoryRepository;
        private readonly IMapper mapper;

        public GetCategoriesQueryHandler(IEventCategoryRepository categoryRepository, IMapper mapper)
        {
            this.categoryRepository = categoryRepository;
            this.mapper = mapper;
        }

        public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var categories = request.ActiveOnly
                    ? await this.categoryRepository.GetActiveAsync(cancellationToken)
                    : await this.categoryRepository.GetAllAsync(cancellationToken);

                var categoryDtos = this.mapper.Map<List<CategoryDto>>(categories);
                return Result.Success(categoryDtos);
            }
            catch (Exception ex)
            {
                return Result.Failure<List<CategoryDto>>($"Failed to retrieve categories: {ex.Message}");
            }
        }
    }
}
