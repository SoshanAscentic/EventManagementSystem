// <copyright file="DatabaseSeeder.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence
{
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.ValueObjects;
    using EventManagementSystem.Persistence.Context;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<DatabaseSeeder> logger;

        public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                this.logger.LogInformation("=== Starting database seeding ===");

                // Check database connection
                if (!await this.context.Database.CanConnectAsync())
                {
                    this.logger.LogError("Cannot connect to database!");
                    return;
                }

                this.logger.LogInformation("Database connection successful");

                await this.SeedCategoriesAsync();
                await this.SeedUsersAsync();
                await this.SeedEventsAsync();

                var changes = await this.context.SaveChangesAsync();
                this.logger.LogInformation("=== Database seeding completed successfully. {Changes} changes saved ===", changes);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "=== Error occurred during database seeding ===");
                throw;
            }
        }

        private async Task SeedCategoriesAsync()
        {
            var existingCount = await this.context.EventCategories.CountAsync();
            this.logger.LogInformation("Existing categories count: {Count}", existingCount);

            if (existingCount > 0)
            {
                this.logger.LogInformation("Categories already exist, skipping seeding");
                return;
            }

            this.logger.LogInformation("Seeding categories...");

            var categories = new[]
            {
                EventCategory.Create("Technology", "Technology conferences, workshops, and tech talks"),
                EventCategory.Create("Business", "Business seminars, networking events, and corporate meetings"),
                EventCategory.Create("Health & Wellness", "Health seminars, fitness events, and wellness workshops"),
                EventCategory.Create("Education", "Educational workshops, training sessions, and academic conferences"),
                EventCategory.Create("Arts & Culture", "Art exhibitions, cultural events, and creative workshops"),
                EventCategory.Create("Sports", "Sports events, competitions, and fitness activities"),
                EventCategory.Create("Entertainment", "Entertainment events, shows, and social gatherings"),
                EventCategory.Create("Community", "Community events, volunteer activities, and local gatherings"),
            };

            await this.context.EventCategories.AddRangeAsync(categories);
            this.logger.LogInformation("Added {Count} categories to context", categories.Length);

            // Save categories first so they get IDs
            var categorySaveResult = await this.context.SaveChangesAsync();
            this.logger.LogInformation("Saved {Count} categories to database", categorySaveResult);
        }

        private async Task SeedUsersAsync()
        {
            var existingCount = await this.context.Users.CountAsync();
            this.logger.LogInformation("Existing users count: {Count}", existingCount);

            if (existingCount > 0)
            {
                this.logger.LogInformation("Users already exist, skipping seeding");
                return;
            }

            this.logger.LogInformation("Seeding users...");

            var users = new[]
            {
                User.Create("admin@eventmanagement.com", "Admin", "User", "+1234567890"),
                User.Create("john.doe@example.com", "John", "Doe", "+1234567891"),
                User.Create("jane.smith@example.com", "Jane", "Smith", "+1234567892"),
                User.Create("bob.johnson@example.com", "Bob", "Johnson", "+1234567893"),
                User.Create("alice.williams@example.com", "Alice", "Williams", "+1234567894"),
            };

            await this.context.Users.AddRangeAsync(users);
            this.logger.LogInformation("Added {Count} users to context", users.Length);

            // Save users first so they get IDs
            var userSaveResult = await this.context.SaveChangesAsync();
            this.logger.LogInformation("Saved {Count} users to database", userSaveResult);
        }

        private async Task SeedEventsAsync()
        {
            var existingCount = await this.context.Events.CountAsync();
            this.logger.LogInformation("Existing events count: {Count}", existingCount);

            if (existingCount > 0)
            {
                this.logger.LogInformation("Events already exist, skipping seeding");
                return;
            }

            this.logger.LogInformation("Seeding events...");

            // Get categories from database (they should exist now)
            var categories = await this.context.EventCategories.ToListAsync();
            if (!categories.Any())
            {
                this.logger.LogWarning("No categories found in database, cannot seed events");
                return;
            }

            this.logger.LogInformation("Found {Count} categories for event seeding", categories.Count);

            var now = DateTime.UtcNow;
            var events = new List<Event>();

            // Technology events
            var techCategory = categories.FirstOrDefault(c => c.Name == "Technology");
            if (techCategory != null)
            {
                this.logger.LogInformation("Creating technology events...");
                events.AddRange(new[]
                {
                    Event.Create(
                        "Annual Tech Conference 2024",
                        "Join us for the biggest technology conference of the year featuring keynotes from industry leaders, hands-on workshops, and networking opportunities.",
                        now.AddDays(30),
                        now.AddDays(30).AddHours(8),
                        "Convention Center",
                        "123 Tech Street, Silicon Valley",
                        500,
                        EventType.Conference,
                        techCategory.Id,
                        "Silicon Valley",
                        "USA"),

                    Event.Create(
                        "AI Workshop: Introduction to Machine Learning",
                        "Learn the fundamentals of machine learning and artificial intelligence in this hands-on workshop designed for beginners.",
                        now.AddDays(15),
                        now.AddDays(15).AddHours(4),
                        "Tech Hub",
                        "456 Innovation Ave, Tech City",
                        50,
                        EventType.Workshop,
                        techCategory.Id,
                        "Tech City",
                        "USA"),
                });
            }

            // Business events
            var businessCategory = categories.FirstOrDefault(c => c.Name == "Business");
            if (businessCategory != null)
            {
                this.logger.LogInformation("Creating business events...");
                events.AddRange(new[]
                {
                    Event.Create(
                        "Entrepreneurship Summit",
                        "Connect with successful entrepreneurs, investors, and business leaders. Learn strategies for building and scaling your business.",
                        now.AddDays(25),
                        now.AddDays(25).AddHours(6),
                        "Business Center",
                        "321 Business Blvd, Commerce City",
                        200,
                        EventType.Conference,
                        businessCategory.Id,
                        "Commerce City",
                        "USA"),
                });
            }

            if (events.Any())
            {
                await this.context.Events.AddRangeAsync(events);
                this.logger.LogInformation("Added {Count} events to context", events.Count);

                var eventSaveResult = await this.context.SaveChangesAsync();
                this.logger.LogInformation("Saved {Count} events to database", eventSaveResult);
            }
            else
            {
                this.logger.LogWarning("No events were created");
            }
        }
    }
}
