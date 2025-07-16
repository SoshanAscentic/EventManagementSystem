// <copyright file="EventType.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Entities
{
    public sealed class EventType : ValueObject
    {
        public static readonly EventType Conference = new ("Conference");
        public static readonly EventType Workshop = new ("Workshop");
        public static readonly EventType Seminar = new ("Seminar");
        public static readonly EventType Webinar = new ("Webinar");
        public static readonly EventType Networking = new ("Networking");
        public static readonly EventType Training = new ("Training");
        public static readonly EventType Social = new ("Social");
        public static readonly EventType Competition = new ("Competition");

        private EventType(string value)
        {
            this.Value = value;
        }

        public string Value { get; private set; }

        public static implicit operator string(EventType eventType) => eventType.Value;

        public static EventType Create(string type)
        {
            return type?.ToLowerInvariant() switch
            {
                "conference" => Conference,
                "workshop" => Workshop,
                "seminar" => Seminar,
                "webinar" => Webinar,
                "networking" => Networking,
                "training" => Training,
                "social" => Social,
                "competition" => Competition,
                _ => throw new ArgumentException($"Invalid event type: {type}", nameof(type))
            };
        }

        public static IReadOnlyList<EventType> GetAllTypes()
        {
            return new List<EventType>
            {
                Conference, Workshop, Seminar, Webinar,
                Networking, Training, Social, Competition,
            };
        }

        public override string ToString() => this.Value;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}
