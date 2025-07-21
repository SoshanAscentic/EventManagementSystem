// <copyright file="EventId.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.ValueObjects
{
    using EventManagementSystem.Domain.Common;

    public sealed class EventId : ValueObject
    {
        private EventId(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Event ID cannot be negative.");
            }

            this.Value = value;
        }

        public int Value { get; private set; }

        public static implicit operator int(EventId eventId) => eventId.Value;

        public static explicit operator EventId(int value) => Create(value);

        public static EventId Create(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentException("EventId must be positive", nameof(value));
            }

            return new EventId(value);
        }

        public static EventId CreateNew() => new(0);

        public override string ToString() => this.Value.ToString();

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}