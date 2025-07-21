// <copyright file="RegistrationStatus.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.ValueObjects
{
    using EventManagementSystem.Domain.Common;

    public sealed class RegistrationStatus : ValueObject
    {
        public static readonly RegistrationStatus Registered = new ("Registered");
        public static readonly RegistrationStatus Cancelled = new ("Cancelled");
        public static readonly RegistrationStatus Attended = new ("Attended");
        public static readonly RegistrationStatus NoShow = new ("NoShow");

        private RegistrationStatus(string value)
        {
            this.Value = value;
        }

        public string Value { get; private set; }

        public bool IsActive => this == Registered;

        public bool IsCancelled => this == Cancelled;

        public bool IsCompleted => this == Attended || this == NoShow;

        public static implicit operator string(RegistrationStatus status) => status.Value;

        public static RegistrationStatus Create(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "registered" => Registered,
                "cancelled" => Cancelled,
                "attended" => Attended,
                "noshow" => NoShow,
                _ => throw new ArgumentException($"Invalid registration status: {status}", nameof(status))
            };
        }

        public override string ToString() => this.Value;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}
