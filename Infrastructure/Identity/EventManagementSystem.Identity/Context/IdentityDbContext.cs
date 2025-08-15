// <copyright file="IdentityDbContext.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Identity.Context
{
    using EventManagementSystem.Identity.Entities;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class IdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Identity table names
            modelBuilder.Entity<ApplicationUser>().ToTable("Users", "Identity");
            modelBuilder.Entity<ApplicationRole>().ToTable("Roles", "Identity");
            modelBuilder.Entity<IdentityUserRole<int>>().ToTable("UserRoles", "Identity");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims", "Identity");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins", "Identity");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens", "Identity");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims", "Identity");

            // Configure ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Phone)
                    .HasMaxLength(20);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.UpdatedAt)
                    .IsRequired();

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.HasIndex(e => new { e.FirstName, e.LastName })
                    .HasDatabaseName("IX_Identity_Users_FirstName_LastName");

                entity.HasIndex(e => e.DomainUserId)
                    .IsUnique()
                    .HasFilter("[DomainUserId] IS NOT NULL");

                // Ignore computed properties - only ignore the ones that actually exist
                entity.Ignore(e => e.FullName);
            });

            // Configure ApplicationRole
            modelBuilder.Entity<ApplicationRole>(entity =>
            {
                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                    .IsRequired();
            });

            // Configure RefreshToken
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens", "Identity");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedByIp)
                    .IsRequired()
                    .HasMaxLength(45);

                entity.Property(e => e.RevokedByIp)
                    .HasMaxLength(45);

                entity.Property(e => e.ReplacedByToken)
                    .HasMaxLength(500);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.Token)
                    .IsUnique();

                entity.HasIndex(e => e.UserId);

                // Ignore computed properties
                entity.Ignore(e => e.IsExpired);
                entity.Ignore(e => e.IsRevoked);
                entity.Ignore(e => e.IsActive);
            });
        }
    }
}
