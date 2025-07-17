// <copyright file="GetCategoryQuery.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Queries.GetCategory
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Application.DTOs;
    using MediatR;

    public class GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>
    {
        public bool ActiveOnly { get; set; } = true;
    }
}
