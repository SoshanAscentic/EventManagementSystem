// <copyright file="PagedResult.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Models
{
    public class PagedResult<T>
    {
        public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            this.Items = items?.ToList() ?? new List<T>();
            this.TotalCount = totalCount;
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            this.HasPreviousPage = pageNumber > 1;
            this.HasNextPage = pageNumber < this.TotalPages;
        }

        public List<T> Items { get; }

        public int TotalCount { get; }

        public int PageNumber { get; }

        public int PageSize { get; }

        public int TotalPages { get; }

        public bool HasPreviousPage { get; }

        public bool HasNextPage { get; }

        public static PagedResult<T> Create(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        {
            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        public static PagedResult<T> Empty(int pageNumber, int pageSize)
        {
            return new PagedResult<T>(Enumerable.Empty<T>(), 0, pageNumber, pageSize);
        }
    }
}
