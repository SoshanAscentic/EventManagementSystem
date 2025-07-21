// <copyright file="ModelBuilderExtensions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Extensions
{
    using System.Text.RegularExpressions;
    using Microsoft.EntityFrameworkCore;

    public static class ModelBuilderExtensions
    {
        public static void ApplyNamingConventions(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Convert entity names to snake_case
                entity.SetTableName(ToSnakeCase(entity.GetTableName() ?? entity.DisplayName()));

                // Convert property names to snake_case
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(ToSnakeCase(property.GetColumnName()));
                }

                // Convert foreign key names to snake_case
                foreach (var key in entity.GetForeignKeys())
                {
                    key.SetConstraintName(ToSnakeCase(key.GetConstraintName() ?? string.Empty));
                }

                // Convert index names to snake_case
                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName() ?? string.Empty));
                }
            }
        }

        public static void ApplyPerformanceOptimizations(this ModelBuilder modelBuilder)
        {
            // Add performance optimizations
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Set default string length to prevent VARCHAR(MAX)
                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(string) && !property.GetMaxLength().HasValue)
                    {
                        property.SetMaxLength(255);
                    }
                }
            }
        }

        private static string ToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var startUnderscores = Regex.Match(input, @"^_+");
            return startUnderscores + Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
        }
    }
}
