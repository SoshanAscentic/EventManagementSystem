// <copyright file="IApplicationDbContext.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Interfaces
{
    using EventManagementSystem.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }

        DbSet<Event> Events { get; }

        DbSet<EventRegistration> EventRegistrations { get; }

        DbSet<EventCategory> EventCategories { get; }

        DbSet<EventImage> EventImages { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
