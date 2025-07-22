// <copyright file="IdentityDesignTimeDbContextFactory.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Identity.Context
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.Extensions.Configuration;

    public class IdentityDesignTimeDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
    {
        public IdentityDbContext CreateDbContext(string[] args)
        {
            // Build configuration by reading from appsettings.json
            // You might need to adjust the path based on your project structure
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Presentation", "EventManagementSystem.Api");

            // If the above path doesn't work, try this alternative
            if (!Directory.Exists(basePath))
            {
                basePath = Directory.GetCurrentDirectory();

                // Walk up directories to find appsettings.json
                while (!File.Exists(Path.Combine(basePath, "appsettings.json")) && Directory.GetParent(basePath) != null)
                {
                    basePath = Directory.GetParent(basePath) !.FullName;
                }
            }

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            // Get the connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }

            // Setup the DbContextOptions
            var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
            });

            return new IdentityDbContext(optionsBuilder.Options);
        }
    }
}
