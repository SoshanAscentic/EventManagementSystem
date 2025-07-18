// <copyright file="CacheKeys.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Constants
{
    public static class CacheKeys
    {
        public const string EVENT_PREFIX = "event:";
        public const string USER_PREFIX = "user:";
        public const string CATEGORY_PREFIX = "category:";
        public const string REGISTRATION_PREFIX = "registration:";

        public static string Event(int id) => $"{EVENT_PREFIX}{id}";
        public static string EventWithDetails(int id) => $"{EVENT_PREFIX}details:{id}";
        public static string User(int id) => $"{USER_PREFIX}{id}";
        public static string UserByEmail(string email) => $"{USER_PREFIX}email:{email}";
        public static string Categories() => "categories:all";
        public static string ActiveCategories() => "categories:active";
        public static string UpcomingEvents(int? categoryId = null) =>
            categoryId.HasValue ? $"events:upcoming:category:{categoryId}" : "events:upcoming";

        public const int DEFAULT_CACHE_DURATION_MINUTES = 30;
        public const int LONG_CACHE_DURATION_MINUTES = 60;
        public const int SHORT_CACHE_DURATION_MINUTES = 5;
    }
}
