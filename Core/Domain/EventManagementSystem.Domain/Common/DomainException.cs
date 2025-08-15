// <copyright file="DomainException.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Common
{
    using System;

    public abstract class DomainException : Exception
    {
        protected DomainException(string errorCode, string message)
        : base(message)
        {
            this.ErrorCode = errorCode;
        }

        protected DomainException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }

        public string ErrorCode { get; }

        public override string ToString()
        {
            return $"[{this.ErrorCode}] {this.GetType().Name}: {this.Message}";
        }
    }
}
