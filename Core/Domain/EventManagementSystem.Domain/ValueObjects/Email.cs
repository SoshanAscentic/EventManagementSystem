// <copyright file="Email.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.ValueObjects
{
    using System.Text.RegularExpressions;
    using EventManagementSystem.Domain.Common;

    public sealed class Email : ValueObject
    {
        private static readonly Regex EmailRegex = new (
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private Email(string value)
        {
            this.Value = value;
        }

        public string Value { get; private set; }

        public static implicit operator string(Email email) => email.Value;

        public static explicit operator Email(string value) => Create(value);

        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or empty", nameof(email));
            }

            var trimmedEmail = email.Trim().ToLowerInvariant();

            if (trimmedEmail.Length > 254)
            {
                throw new ArgumentException("Email cannot exceed 254 characters", nameof(email));
            }

            if (!EmailRegex.IsMatch(trimmedEmail))
            {
                throw new ArgumentException("Invalid email format", nameof(email));
            }

            return new Email(trimmedEmail);
        }

        public override string ToString() => this.Value;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}
