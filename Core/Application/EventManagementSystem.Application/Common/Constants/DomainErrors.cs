// <copyright file="DomainErrors.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Constants
{
    using EventManagementSystem.Application.Common.Models;

    public static class DomainErrors
    {
        public static class Event
        {
            public static Error NotFound(int eventId) =>
                Error.NotFound("Event.NotFound", $"Event with ID {eventId} was not found");

            public static Error TitleEmpty() =>
                Error.Validation("Event.TitleEmpty", "Event title cannot be empty");

            public static Error TitleTooLong(int maxLength) =>
                Error.Validation("Event.TitleTooLong", $"Event title cannot exceed {maxLength} characters");

            public static Error DescriptionEmpty() =>
                Error.Validation("Event.DescriptionEmpty", "Event description cannot be empty");

            public static Error DescriptionTooLong(int maxLength) =>
                Error.Validation("Event.DescriptionTooLong", $"Event description cannot exceed {maxLength} characters");

            public static Error InvalidDateRange() =>
                Error.Validation("Event.InvalidDateRange", "Event end time must be after start time");

            public static Error PastStartDate() =>
                Error.Validation("Event.PastStartDate", "Event start time cannot be in the past");

            public static Error InvalidCapacity() =>
                Error.Validation("Event.InvalidCapacity", "Event capacity must be between 1 and 10,000");

            public static Error CapacityExceeded(int eventId, int currentRegistrations, int capacity) =>
                Error.Conflict(
                    "Event.CapacityExceeded",
                    $"Event {eventId} is at full capacity ({currentRegistrations}/{capacity})");

            public static Error RegistrationClosed(int eventId) =>
                Error.Conflict(
                    "Event.RegistrationClosed",
                    $"Registration for event {eventId} is closed");

            public static Error AlreadyStarted(int eventId) =>
                Error.Conflict(
                    "Event.AlreadyStarted",
                    $"Cannot modify event {eventId} that has already started");

            public static Error HasActiveRegistrations(int eventId, int registrationCount) =>
                Error.Conflict(
                    "Event.HasActiveRegistrations",
                    $"Cannot delete event {eventId} with {registrationCount} active registrations");

            public static Error DuplicateTitle(string title, DateTime date) =>
                Error.Conflict(
                    "Event.DuplicateTitle",
                    $"An event with title '{title}' already exists on {date:yyyy-MM-dd}");

            public static Error InvalidEventType(string eventType) =>
                Error.Validation(
                    "Event.InvalidEventType",
                    $"Invalid event type '{eventType}'. Valid types are: Conference, Workshop, Seminar, etc.");

            public static Error CategoryNotFound(int categoryId) =>
                Error.NotFound(
                    "Event.CategoryNotFound",
                    $"Event category with ID {categoryId} was not found");

            public static Error ImageNotFound(int imageId) =>
                Error.NotFound(
                    "Event.ImageNotFound",
                    $"Event image with ID {imageId} was not found");

            public static Error PrimaryImageAlreadyExists() =>
                Error.Conflict(
                    "Event.PrimaryImageAlreadyExists",
                    "Event already has a primary image");
        }

        public static class User
        {
            public static Error NotFound(int userId) =>
                Error.NotFound("User.NotFound", $"User with ID {userId} was not found");

            public static Error NotFoundByEmail(string email) =>
                Error.NotFound("User.NotFoundByEmail", $"User with email '{email}' was not found");

            public static Error EmailAlreadyExists(string email) =>
                Error.Conflict(
                    "User.EmailAlreadyExists",
                    $"A user with email '{email}' already exists");

            public static Error InvalidEmail(string email) =>
                Error.Validation("User.InvalidEmail", $"Invalid email format: '{email}'");

            public static Error FirstNameEmpty() =>
                Error.Validation("User.FirstNameEmpty", "First name cannot be empty");

            public static Error LastNameEmpty() =>
                Error.Validation("User.LastNameEmpty", "Last name cannot be empty");

            public static Error NameTooLong(int maxLength) =>
                Error.Validation(
                    "User.NameTooLong",
                    $"First name and last name cannot exceed {maxLength} characters each");

            public static Error InvalidPhone(string phone) =>
                Error.Validation("User.InvalidPhone", $"Invalid phone number format: '{phone}'");

            public static Error AccountDeactivated(int userId) =>
                Error.Forbidden(
                    "User.AccountDeactivated",
                    $"User account {userId} is deactivated");
        }

        public static class Registration
        {
            public static Error NotFound(int registrationId) =>
                Error.NotFound(
                    "Registration.NotFound",
                    $"Registration with ID {registrationId} was not found");

            public static Error UserAlreadyRegistered(int userId, int eventId) =>
                Error.Conflict(
                    "Registration.UserAlreadyRegistered",
                    $"User {userId} is already registered for event {eventId}");

            public static Error CannotCancelPastDeadline(int registrationId, DateTime deadline) =>
                Error.Conflict(
                    "Registration.CannotCancelPastDeadline",
                    $"Cannot cancel registration {registrationId} after deadline {deadline:yyyy-MM-dd HH:mm}");

            public static Error AlreadyCancelled(int registrationId, DateTime value) =>
                Error.Conflict(
                    "Registration.AlreadyCancelled",
                    $"Registration {registrationId} is already cancelled");

            public static Error AlreadyAttended(int registrationId) =>
                Error.Conflict(
                    "Registration.AlreadyAttended",
                    $"Registration {registrationId} is already marked as attended");

            public static Error CannotMarkAttendedForCancelled(int registrationId) =>
                Error.Conflict(
                    "Registration.CannotMarkAttendedForCancelled",
                    $"Cannot mark cancelled registration {registrationId} as attended");

            public static Error EventNotStarted(int eventId) =>
                Error.Conflict(
                    "Registration.EventNotStarted",
                    $"Cannot mark attendance for event {eventId} that hasn't started");

            public static Error InvalidStatus(string status) =>
                Error.Validation(
                    "Registration.InvalidStatus",
                    $"Invalid registration status: '{status}'");
        }

        public static class Category
        {
            public static Error NotFound(int categoryId) =>
                Error.NotFound(
                    "Category.NotFound",
                    $"Category with ID {categoryId} was not found");

            public static Error NameAlreadyExists(string name) =>
                Error.Conflict(
                    "Category.NameAlreadyExists",
                    $"A category with name '{name}' already exists");

            public static Error NameEmpty() =>
                Error.Validation("Category.NameEmpty", "Category name cannot be empty");

            public static Error NameTooLong(int maxLength) =>
                Error.Validation(
                    "Category.NameTooLong",
                    $"Category name cannot exceed {maxLength} characters");

            public static Error DescriptionEmpty() =>
                Error.Validation("Category.DescriptionEmpty", "Category description cannot be empty");

            public static Error DescriptionTooLong(int maxLength) =>
                Error.Validation(
                    "Category.DescriptionTooLong",
                    $"Category description cannot exceed {maxLength} characters");

            public static Error HasActiveEvents(int categoryId, int eventCount) =>
                Error.Conflict(
                    "Category.HasActiveEvents",
                    $"Cannot delete category {categoryId} with {eventCount} active events");

            public static Error IsInactive(int categoryId) =>
                Error.Conflict(
                    "Category.IsInactive",
                    $"Category {categoryId} is inactive and cannot be used");
        }

        public static class File
        {
            public static Error NotFound(string fileName) =>
                Error.NotFound("File.NotFound", $"File '{fileName}' was not found");

            public static Error InvalidFormat(string fileName, string[] allowedFormats) =>
                Error.Validation(
                    "File.InvalidFormat",
                    $"File '{fileName}' has invalid format. Allowed formats: {string.Join(", ", allowedFormats)}");

            public static Error SizeExceeded(string fileName, long actualSize, long maxSize) =>
                Error.Validation(
                    "File.SizeExceeded",
                    $"File '{fileName}' size ({actualSize} bytes) exceeds maximum allowed size ({maxSize} bytes)");

            public static Error UploadFailed(string fileName, string reason) =>
                Error.Failure(
                    "File.UploadFailed",
                    $"Failed to upload file '{fileName}': {reason}");

            public static Error DeleteFailed(string fileName, string reason) =>
                Error.Failure(
                    "File.DeleteFailed",
                    $"Failed to delete file '{fileName}': {reason}");

            public static Error EmptyFile() =>
                Error.Validation("File.EmptyFile", "File cannot be empty");

            public static Error InvalidContentType(string contentType) =>
                Error.Validation(
                    "File.InvalidContentType",
                    $"Invalid content type: '{contentType}'");
        }

        public static class General
        {
            public static Error InvalidId(string entityName) =>
                Error.Validation(
                    "General.InvalidId",
                    $"{entityName} ID must be a positive integer");

            public static Error DatabaseError(string operation) =>
                Error.Failure(
                    "General.DatabaseError",
                    $"Database error occurred during {operation}. Please try again");

            public static Error UnexpectedError() =>
                Error.Failure(
                    "General.UnexpectedError",
                    "An unexpected error occurred. Please try again later");

            public static Error ValidationFailed(string details) =>
                Error.Validation(
                    "General.ValidationFailed",
                    $"Validation failed: {details}");

            public static Error OperationNotAllowed(string operation) =>
                Error.Forbidden(
                    "General.OperationNotAllowed",
                    $"Operation '{operation}' is not allowed");

            public static Error ConcurrencyConflict() =>
                Error.Conflict(
                    "General.ConcurrencyConflict",
                    "The data was modified by another process. Please refresh and try again");
        }

        public static class Authentication
        {
            public static Error InvalidCredentials() =>
                Error.Unauthorized("Auth.InvalidCredentials", "Invalid email or password");

            public static Error TokenExpired() =>
                Error.Unauthorized("Auth.TokenExpired", "Authentication token has expired");

            public static Error TokenInvalid() =>
                Error.Unauthorized("Auth.TokenInvalid", "Invalid authentication token");

            public static Error AccessDenied() =>
                Error.Forbidden("Auth.AccessDenied", "Access denied for this resource");

            public static Error InsufficientPermissions(string operation) =>
                Error.Forbidden(
                    "Auth.InsufficientPermissions",
                    $"Insufficient permissions to perform '{operation}'");

            public static Error AccountLocked() =>
                Error.Forbidden(
                    "Auth.AccountLocked",
                    "Account is locked due to multiple failed login attempts");

            public static Error EmailNotConfirmed() =>
                Error.Forbidden(
                    "Auth.EmailNotConfirmed",
                    "Email address must be confirmed before accessing this resource");
        }
    }
}
