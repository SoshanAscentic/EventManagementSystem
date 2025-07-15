// <copyright file="Phone.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.ValueObjects
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public sealed class Phone : ValueObject
    {
        private static readonly Regex PhoneRegex = new (
            @"^\+?[\d\s\-\(\)]{10,15}$",
            RegexOptions.Compiled);

        private Phone(string value)
        {
            this.Value = value;
        }

        public string Value { get; private set; }

        public static implicit operator string(Phone phone) => phone.Value;

        public static explicit operator Phone(string value) => Create(value);

        public static Phone Create(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                throw new ArgumentException("Phone cannot be null or empty", nameof(phone));
            }

            var trimmedPhone = phone.Trim();

            if (!PhoneRegex.IsMatch(trimmedPhone))
            {
                throw new ArgumentException("Invalid phone format", nameof(phone));
            }

            return new Phone(trimmedPhone);
        }

        public override string ToString() => this.Value;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}
