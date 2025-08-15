// <copyright file="EventCapacity.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.ValueObjects
{
    using EventManagementSystem.Domain.Common;

    public sealed class EventCapacity : ValueObject
    {
        public const int MinCapacity = 1;
        public const int MaxCapacity = 10000;

        // Private parameterless constructor for EF Core
        private EventCapacity()
        {
            this.Value = 1;
        }

        private EventCapacity(int value)
        {
            this.Value = value;
        }

        public int Value { get; private set; }

        public static implicit operator int(EventCapacity capacity) => capacity.Value;

        public static explicit operator EventCapacity(int value) => Create(value);

        public static EventCapacity Create(int capacity)
        {
            if (capacity < MinCapacity || capacity > MaxCapacity)
            {
                throw new ArgumentException(
                    $"Event capacity must be between {MinCapacity} and {MaxCapacity}",
                    nameof(capacity));
            }

            return new EventCapacity(capacity);
        }

        public bool IsFull(int currentRegistrations)
        {
            return currentRegistrations >= this.Value;
        }

        public int RemainingCapacity(int currentRegistrations)
        {
            return Math.Max(0, this.Value - currentRegistrations);
        }

        public override string ToString() => this.Value.ToString();

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}
