// <copyright file="CategoryExceptions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Exceptions
{
    using EventManagementSystem.Domain.Common;

    public sealed class CategoryNotFoundException : DomainException
    {
        public CategoryNotFoundException(int categoryId)
                : base("Category.NotFound", $"Event category with ID {categoryId} was not found.")
        {
            this.CategoryId = categoryId;
        }

        public int CategoryId { get; }
    }

    public sealed class DuplicateCategoryNameException : DomainException
    {
        public DuplicateCategoryNameException(string categoryName)
                : base("Category.DuplicateName", $"A category with name '{categoryName}' already exists.")
        {
            this.CategoryName = categoryName;
        }

        public string CategoryName { get; }
    }

    public sealed class CategoryHasEventsException : DomainException
    {
        public CategoryHasEventsException(int categoryId, int eventCount)
                : base(
                    "Category.HasEvents",
                    $"Cannot delete category {categoryId} because it has {eventCount} associated events.")
        {
            this.CategoryId = categoryId;
            this.EventCount = eventCount;
        }

        public int CategoryId { get; }

        public int EventCount { get; }
        }

    public sealed class InactiveCategoryException : DomainException
    {
        public InactiveCategoryException(int categoryId)
                : base("Category.Inactive", $"Category {categoryId} is inactive and cannot be used.")
        {
            this.CategoryId = categoryId;
        }

        public int CategoryId { get; }
    }
}
