// <copyright file="EventType.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.ValueObjects
{
    using EventManagementSystem.Domain.Common;

    public sealed class EventType : ValueObject
    {
        public static readonly EventType Conference = new ("Conference");
        public static readonly EventType Workshop = new ("Workshop");
        public static readonly EventType Seminar = new ("Seminar");
        public static readonly EventType Webinar = new ("Webinar");
        public static readonly EventType Meeting = new ("Meeting");
        public static readonly EventType Training = new ("Training");
        public static readonly EventType Networking = new ("Networking");
        public static readonly EventType Social = new ("Social");
        public static readonly EventType Competition = new ("Competition");
        public static readonly EventType Exhibition = new ("Exhibition");
        public static readonly EventType Performance = new ("Performance");
        public static readonly EventType Festival = new ("Festival");
        public static readonly EventType Sports = new ("Sports");
        public static readonly EventType Charity = new ("Charity");
        public static readonly EventType Other = new ("Other");

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
                "meeting" => Meeting,
                "training" => Training,
                "networking" => Networking,
                "social" => Social,
                "competition" => Competition,
                "exhibition" => Exhibition,
                "performance" => Performance,
                "festival" => Festival,
                "sports" => Sports,
                "charity" => Charity,
                "other" => Other,
                _ => throw new ArgumentException($"Invalid event type: {type}", nameof(type))
            };
        }

        public static IReadOnlyList<EventType> GetAllTypes()
        {
            return new List<EventType>
            {
                Conference, Workshop, Seminar, Webinar, Meeting,
                Training, Networking, Social, Competition, Exhibition,
                Performance, Festival, Sports, Charity, Other,
            };
        }

        // Add extension methods directly to the value object
        public string GetDisplayName()
        {
            return this.Value;
        }

        public string GetDescription()
        {
            return this.Value switch
            {
                "Conference" => "Large formal meeting with presentations and discussions",
                "Workshop" => "Interactive learning session with hands-on activities",
                "Seminar" => "Educational presentation on a specific topic",
                "Webinar" => "Online seminar or presentation",
                "Meeting" => "Formal gathering for discussion or decision-making",
                "Training" => "Structured learning program to develop skills",
                "Networking" => "Event focused on building professional connections",
                "Social" => "Casual gathering for social interaction",
                "Competition" => "Contest or competitive event",
                "Exhibition" => "Display of items or achievements",
                "Performance" => "Artistic or entertainment presentation",
                "Festival" => "Celebration or series of events",
                "Sports" => "Athletic competition or sports event",
                "Charity" => "Fundraising or charitable event",
                "Other" => "Other type of event",
                _ => "Unknown event type"
            };
        }

        public override string ToString() => this.Value;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}
