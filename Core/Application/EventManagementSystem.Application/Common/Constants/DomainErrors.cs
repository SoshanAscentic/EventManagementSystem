// <copyright file="DomainErrors.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Constants
{
    public static class DomainErrors
    {
        public static class Event
        {
            public const string NotFound = "Event not found";
            public const string CapacityExceeded = "Event capacity exceeded";
            public const string RegistrationClosed = "Registration is closed for this event";
            public const string PastEvent = "Cannot perform operation on past event";
            public const string HasActiveRegistrations = "Event has active registrations";
        }

        public static class User
        {
            public const string NotFound = "User not found";
            public const string EmailAlreadyExists = "User with this email already exists";
            public const string InvalidCredentials = "Invalid credentials";
        }

        public static class Registration
        {
            public const string NotFound = "Registration not found";
            public const string AlreadyRegistered = "User is already registered for this event";
            public const string CannotCancel = "Cannot cancel registration";
            public const string DeadlinePassed = "Registration deadline has passed";
        }

        public static class Category
        {
            public const string NotFound = "Category not found";
            public const string NameAlreadyExists = "Category with this name already exists";
            public const string HasEvents = "Category has associated events";
        }

        public static class File
        {
            public const string InvalidFormat = "Invalid file format";
            public const string SizeExceeded = "File size exceeded";
            public const string UploadFailed = "File upload failed";
        }
    }
}
