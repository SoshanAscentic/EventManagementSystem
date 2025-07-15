// <copyright file="EventLocation.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.ValueObjects
{
    public sealed class EventLocation : ValueObject
    {
        private EventLocation(string venue, string address, string? city = null, string? country = null)
        {
            this.Venue = venue;
            this.Address = address;
            this.City = city;
            this.Country = country;
        }

        public string Venue { get; private set; }

        public string Address { get; private set; }

        public string? City { get; private set; }

        public string? Country { get; private set; }

        public string FullAddress => $"{this.Venue}, {this.Address}" +
            (string.IsNullOrEmpty(this.City) ? string.Empty : $", {this.City}") +
            (string.IsNullOrEmpty(this.Country) ? string.Empty : $", {this.Country}");

        public static EventLocation Create(string venue, string address, string? city = null, string? country = null)
        {
            if (string.IsNullOrWhiteSpace(venue))
            {
                throw new ArgumentException("Venue cannot be null or empty", nameof(venue));
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Address cannot be null or empty", nameof(address));
            }

            var trimmedVenue = venue.Trim();
            var trimmedAddress = address.Trim();

            if (trimmedVenue.Length > 100)
            {
                throw new ArgumentException("Venue name cannot exceed 100 characters", nameof(venue));
            }

            if (trimmedAddress.Length > 200)
            {
                throw new ArgumentException("Address cannot exceed 200 characters", nameof(address));
            }

            return new EventLocation(
                trimmedVenue,
                trimmedAddress,
                city?.Trim(),
                country?.Trim());
        }

        public override string ToString() => this.FullAddress;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.Venue;
            yield return this.Address;
            yield return this.City;
            yield return this.Country;
        }
    }
}
