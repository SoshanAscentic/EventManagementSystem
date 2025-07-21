// <copyright file="UserId.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.ValueObjects
{
    using EventManagementSystem.Domain.Common;

    public sealed class UserId : ValueObject
    {
        private UserId(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "User ID cannot be negative.");
            }

            this.Value = value;
        }

        public int Value { get; private set; }

        public static implicit operator int(UserId userId) => userId.Value;

        public static explicit operator UserId(int value) => Create(value);

        public static UserId Create(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentException("UserId must be positive", nameof(value));
            }

            return new UserId(value);
        }

        public static UserId CreateNew() => new(0);

        public override string ToString() => this.Value.ToString();

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}