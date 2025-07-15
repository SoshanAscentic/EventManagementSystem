// <copyright file="RegistrationId.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.ValueObjects
{
    using System;
    using System.Collections.Generic;

    public sealed class RegistrationId : ValueObject
    {
        private RegistrationId(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Registration ID cannot be negative.");
            }

            this.Value = value;
        }

        public int Value { get; private set; }

        public static implicit operator int(RegistrationId registrationId) => registrationId.Value;

        public static explicit operator RegistrationId(int value) => Create(value);

        public static RegistrationId Create(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentException("RegistrationId must be positive", nameof(value));
            }

            return new RegistrationId(value);
        }

        public static RegistrationId CreateNew() => new (0);

        public override string ToString() => this.Value.ToString();

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}
