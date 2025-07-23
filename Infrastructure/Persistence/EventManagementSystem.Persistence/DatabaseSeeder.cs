// <copyright file="DatabaseSeeder.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence
{
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.ValueObjects;
    using EventManagementSystem.Identity.Context;
    using EventManagementSystem.Identity.Entities;
    using EventManagementSystem.Persistence.Context;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class DatabaseSeeder
    {
        private readonly ApplicationDbContext context;
        private readonly IdentityDbContext identityContext;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<DatabaseSeeder> logger;

        public DatabaseSeeder(
            ApplicationDbContext context,
            IdentityDbContext identityContext,
            UserManager<ApplicationUser> userManager,
            ILogger<DatabaseSeeder> logger)
        {
            this.context = context;
            this.identityContext = identityContext;
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                this.logger.LogInformation("=== Starting database seeding ===");

                // Check database connections
                if (!await this.context.Database.CanConnectAsync())
                {
                    this.logger.LogError("Cannot connect to main database!");
                    return;
                }

                if (!await this.identityContext.Database.CanConnectAsync())
                {
                    this.logger.LogError("Cannot connect to identity database!");
                    return;
                }

                this.logger.LogInformation("Database connections successful");

                await this.SeedCategoriesAsync();
                await this.SeedUsersAsync(); // This now handles both Identity and Domain users
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

            var categorySaveResult = await this.context.SaveChangesAsync();
            this.logger.LogInformation("Saved {Count} categories to database", categorySaveResult);
        }

        private async Task SeedUsersAsync()
        {
            this.logger.LogInformation("Starting user seeding process...");

            // Check if we already have domain users
            var existingDomainUserCount = await this.context.Users.CountAsync();
            this.logger.LogInformation("Existing domain users count: {Count}", existingDomainUserCount);

            if (existingDomainUserCount > 0)
            {
                this.logger.LogInformation("Domain users already exist, checking synchronization...");
                await this.SynchronizeExistingUsersAsync();
                return;
            }

            this.logger.LogInformation("Seeding new users...");

            // Define seed users (excluding admin since it's already created in Identity seeding)
            var seedUserData = new[]
            {
                new { Email = "john.doe@example.com", FirstName = "John", LastName = "Doe", Phone = "+1234567891", Role = "User" },
                new { Email = "jane.smith@example.com", FirstName = "Jane", LastName = "Smith", Phone = "+1234567892", Role = "User" },
                new { Email = "bob.johnson@example.com", FirstName = "Bob", LastName = "Johnson", Phone = "+1234567893", Role = "User" },
                new { Email = "alice.williams@example.com", FirstName = "Alice", LastName = "Williams", Phone = "+1234567894", Role = "User" },
                new { Email = "eventmanager@example.com", FirstName = "Event", LastName = "Manager", Phone = "+1234567895", Role = "EventManager" },
            };

            foreach (var userData in seedUserData)
            {
                await this.CreateUserWithBothSystemsAsync(userData.Email, userData.FirstName, userData.LastName, userData.Phone, userData.Role);
            }

            this.logger.LogInformation("Completed seeding {Count} new users", seedUserData.Length);
        }

        private async Task SynchronizeExistingUsersAsync()
        {
            this.logger.LogInformation("Synchronizing existing users...");

            // Get all identity users
            var identityUsers = await this.identityContext.Users.ToListAsync();
            this.logger.LogInformation("Found {Count} identity users", identityUsers.Count);

            foreach (var identityUser in identityUsers)
            {
                // Check if domain user exists for this identity user
                Domain.Entities.User? domainUser = null;

                if (identityUser.DomainUserId.HasValue)
                {
                    domainUser = await this.context.Users.FirstOrDefaultAsync(u => u.Id == identityUser.DomainUserId.Value);
                }

                if (domainUser == null)
                {
                    // Try to find by email
                    domainUser = await this.context.Users.FirstOrDefaultAsync(u =>
                        EF.Property<string>(u, "_email") == identityUser.Email);
                }

                if (domainUser == null)
                {
                    // Create domain user for this identity user
                    this.logger.LogInformation("Creating domain user for identity user: {Email}", identityUser.Email);

                    domainUser = Domain.Entities.User.Create(
                        identityUser.Email!,
                        identityUser.FirstName,
                        identityUser.LastName,
                        identityUser.Phone);

                    await this.context.Users.AddAsync(domainUser);
                    await this.context.SaveChangesAsync();

                    // Update identity user with domain user ID
                    identityUser.DomainUserId = domainUser.Id;
                    this.identityContext.Users.Update(identityUser);
                    await this.identityContext.SaveChangesAsync();

                    this.logger.LogInformation(
                        "Linked identity user {IdentityId} with domain user {DomainId}",
                        identityUser.Id,
                        domainUser.Id);
                }
                else if (!identityUser.DomainUserId.HasValue)
                {
                    // Link existing domain user to identity user
                    identityUser.DomainUserId = domainUser.Id;
                    this.identityContext.Users.Update(identityUser);
                    await this.identityContext.SaveChangesAsync();

                    this.logger.LogInformation(
                        "Linked existing domain user {DomainId} with identity user {IdentityId}",
                        domainUser.Id,
                        identityUser.Id);
                }
            }

            this.logger.LogInformation("Synchronization completed");
        }

        private async Task CreateUserWithBothSystemsAsync(string email, string firstName, string lastName, string? phone, string role)
        {
            try
            {
                this.logger.LogInformation("Creating user: {Email}", email);

                // Check if identity user already exists
                var existingIdentityUser = await this.userManager.FindByEmailAsync(email);
                if (existingIdentityUser != null)
                {
                    this.logger.LogInformation("Identity user already exists: {Email}", email);

                    if (!existingIdentityUser.DomainUserId.HasValue)
                    {
                        // Create domain user and link
                        var domainUser = Domain.Entities.User.Create(email, firstName, lastName, phone);
                        await this.context.Users.AddAsync(domainUser);
                        await this.context.SaveChangesAsync();

                        existingIdentityUser.DomainUserId = domainUser.Id;
                        await this.userManager.UpdateAsync(existingIdentityUser);

                        this.logger.LogInformation("Created and linked domain user for existing identity user: {Email}", email);
                    }

                    return;
                }

                // Create identity user
                var identityUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phone,
                    EmailConfirmed = true, // Auto-confirm for seed data
                    IsActive = true,
                };

                var password = "SeedUser123!"; // Use a default password for seed users
                var result = await this.userManager.CreateAsync(identityUser, password);

                if (!result.Succeeded)
                {
                    this.logger.LogError(
                        "Failed to create identity user {Email}: {Errors}",
                        email,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return;
                }

                // Add to role
                await this.userManager.AddToRoleAsync(identityUser, role);

                // Create corresponding domain user
                var domainUserEntity = Domain.Entities.User.Create(email, firstName, lastName, phone);
                await this.context.Users.AddAsync(domainUserEntity);
                await this.context.SaveChangesAsync();

                // Link them
                identityUser.DomainUserId = domainUserEntity.Id;
                await this.userManager.UpdateAsync(identityUser);

                this.logger.LogInformation(
                    "Successfully created and linked user: {Email} (Identity: {IdentityId}, Domain: {DomainId})",
                    email,
                    identityUser.Id,
                    domainUserEntity.Id);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error creating user: {Email}", email);
            }
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
