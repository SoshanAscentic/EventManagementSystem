// <copyright file="MappingExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Mappings
{
    using AutoMapper;
    using EventManagementSystem.Application.Common.Models;

    public static class MappingExtensions
    {
        public static PagedResult<TDestination> MapPagedResult<TSource, TDestination>(
            this IMapper mapper,
            PagedResult<TSource> source)
        {
            var mappedItems = mapper.Map<List<TDestination>>(source.Items);
            return new PagedResult<TDestination>(mappedItems, source.TotalCount, source.PageNumber, source.PageSize);
        }

        public static PagedResult<TDestination> ToPagedResult<TSource, TDestination>(
            this IMapper mapper,
            IEnumerable<TSource> items,
            int totalCount,
            int pageNumber,
            int pageSize)
        {
            var mappedItems = mapper.Map<List<TDestination>>(items);
            return new PagedResult<TDestination>(mappedItems, totalCount, pageNumber, pageSize);
        }

        public static Result<TDestination> MapResult<TSource, TDestination>(
            this IMapper mapper,
            Result<TSource> source)
        {
            if (source.IsFailure)
            {
                return Result<TDestination>.Failure(source.Errors);
            }

            var mappedValue = mapper.Map<TDestination>(source.Value);
            return Result<TDestination>.Success(mappedValue);
        }
    }
}
