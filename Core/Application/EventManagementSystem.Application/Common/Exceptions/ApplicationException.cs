// <copyright file="ApplicationException.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Exceptions
{
    public abstract class ApplicationException : Exception
    {
        protected ApplicationException(string message)
            : base(message)
        {
        }

        protected ApplicationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class ValidationException : ApplicationException
    {
        public ValidationException(IEnumerable<string> failures)
            : base($"Validation failed: {string.Join(", ", failures)}")
        {
            this.Failures = failures.ToArray();
        }

        public string[] Failures { get; }
    }

    public class ForbiddenException : ApplicationException
    {
        public ForbiddenException(string message)
            : base(message)
        {
        }
    }

    public class ConflictException : ApplicationException
    {
        public ConflictException(string message)
            : base(message)
        {
        }
    }
}
