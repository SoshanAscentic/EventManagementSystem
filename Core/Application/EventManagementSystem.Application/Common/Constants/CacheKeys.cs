// <copyright file="CacheKeys.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Constants
{
    public static class CacheKeys
    {
        public const string EVENTPREFIX = "event:";
        public const string USERPREFIX = "user:";
        public const string CATEGORYPREFIX = "category:";
        public const string REGISTRATIONPREFIX = "registration:";

        public const int DEFAULTCACHEDURATIONMINUTES = 30;
        public const int LONGCACHEDURATIONMINUTES = 60;
        public const int SHORTCACHEDURATIONMINUTES = 5;

        public static string Event(int id) => $"{EVENTPREFIX}{id}";

        public static string EventWithDetails(int id) => $"{EVENTPREFIX}details:{id}";

        public static string User(int id) => $"{USERPREFIX}{id}";

        public static string UserByEmail(string email) => $"{USERPREFIX}email:{email}";

        public static string Categories() => "categories:all";

        public static string ActiveCategories() => "categories:active";

        public static string UpcomingEvents(int? categoryId = null) =>
            categoryId.HasValue ? $"events:upcoming:category:{categoryId}" : "events:upcoming";
    }
}
