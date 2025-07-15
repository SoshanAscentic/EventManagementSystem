// <copyright file="EventDateTime.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.ValueObjects
{
    public sealed class EventDateTime : ValueObject
    {
        private EventDateTime(DateTime startDateTime, DateTime endDateTime)
        {
            this.StartDateTime = startDateTime;
            this.EndDateTime = endDateTime;
        }

        public DateTime StartDateTime { get; private set; }

        public DateTime EndDateTime { get; private set; }

        public TimeSpan Duration => this.EndDateTime - this.StartDateTime;

        public bool IsOngoing => DateTime.UtcNow >= this.StartDateTime && DateTime.UtcNow <= this.EndDateTime;

        public bool IsCompleted => DateTime.UtcNow > this.EndDateTime;

        public bool IsUpcoming => DateTime.UtcNow < this.StartDateTime;

        public DateTime RegistrationCutoff => this.StartDateTime.AddHours(-2); // Registration closes 2 hours before

        public bool IsRegistrationOpen => DateTime.UtcNow < this.RegistrationCutoff;

        public static EventDateTime Create(DateTime startDateTime, DateTime endDateTime)
        {
            if (startDateTime < DateTime.UtcNow)
            {
                throw new ArgumentException("Event start time cannot be in the past", nameof(startDateTime));
            }

            if (endDateTime <= startDateTime)
            {
                throw new ArgumentException("Event end time must be after start time", nameof(endDateTime));
            }

            var maxFutureDate = DateTime.UtcNow.AddYears(5);
            if (startDateTime > maxFutureDate)
            {
                throw new ArgumentException("Event cannot be scheduled more than 5 years in advance", nameof(startDateTime));
            }

            return new EventDateTime(startDateTime, endDateTime);
        }

        public static EventDateTime CreateForExisting(DateTime startDateTime, DateTime endDateTime)
        {
            // For existing events, allow past dates (for data loading)
            if (endDateTime <= startDateTime)
            {
                throw new ArgumentException("Event end time must be after start time", nameof(endDateTime));
            }

            return new EventDateTime(startDateTime, endDateTime);
        }

        public override string ToString()
        {
            return $"{this.StartDateTime:yyyy-MM-dd HH:mm} - {this.EndDateTime:yyyy-MM-dd HH:mm}";
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.StartDateTime;
            yield return this.EndDateTime;
        }
    }
}
