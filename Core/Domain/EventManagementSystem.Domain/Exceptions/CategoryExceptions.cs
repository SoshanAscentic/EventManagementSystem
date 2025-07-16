using EventManagementSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Domain.Exceptions
{
    public sealed class CategoryNotFoundException : DomainException
    {
        public int CategoryId { get; }

        public CategoryNotFoundException(int categoryId)
            : base("Category.NotFound", $"Event category with ID {categoryId} was not found.")
        {
            this.CategoryId = categoryId;
        }
    }

    public sealed class DuplicateCategoryNameException : DomainException
    {
        public string CategoryName { get; }

        public DuplicateCategoryNameException(string categoryName)
            : base("Category.DuplicateName", $"A category with name '{categoryName}' already exists.")
        {
            this.CategoryName = categoryName;
        }
    }

    public sealed class CategoryHasEventsException : DomainException
    {
        public int CategoryId { get; }

        public int EventCount { get; }

        public CategoryHasEventsException(int categoryId, int eventCount)
            : base(
                "Category.HasEvents",
                $"Cannot delete category {categoryId} because it has {eventCount} associated events.")
        {
            this.CategoryId = categoryId;
            this.EventCount = eventCount;
        }
    }

    public sealed class InactiveCategoryException : DomainException
    {
        public int CategoryId { get; }

        public InactiveCategoryException(int categoryId)
            : base("Category.Inactive", $"Category {categoryId} is inactive and cannot be used.") => this.CategoryId = categoryId;
    }
}
