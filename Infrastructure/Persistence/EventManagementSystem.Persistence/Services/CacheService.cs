// <copyright file="CacheService.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Persistence.Services
{
    using EventManagementSystem.Application.Common.Interfaces;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;

    public class CacheService : ICacheService
    {
        private readonly IMemoryCache memoryCache;
        private readonly ILogger<CacheService> logger;

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                if (this.memoryCache.TryGetValue(key, out var cachedValue))
                {
                    this.logger.LogDebug("Cache hit for key: {Key}", key);
                    return Task.FromResult((T?)cachedValue);
                }

                this.logger.LogDebug("Cache miss for key: {Key}", key);
                return Task.FromResult(default(T));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error retrieving from cache for key: {Key}", key);
                return Task.FromResult(default(T));
            }
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            try
            {
                var options = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration,
                    SlidingExpiration = TimeSpan.FromMinutes(5), // Sliding expiration of 5 minutes
                };

                this.memoryCache.Set(key, value, options);
                this.logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expiration);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error setting cache for key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                this.memoryCache.Remove(key);
                this.logger.LogDebug("Removed cache for key: {Key}", key);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error removing cache for key: {Key}", key);
                return Task.CompletedTask;
            }
        }

        public Task RemovePatternAsync(string pattern, CancellationToken cancellationToken = default)
        {
            try
            {
                // Note: Memory cache doesn't support pattern removal out of the box
                // For production use, consider using Redis which supports pattern-based removal
                this.logger.LogWarning("Pattern-based cache removal not supported with MemoryCache. Pattern: {Pattern}", pattern);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error removing cache pattern: {Pattern}", pattern);
                return Task.CompletedTask;
            }
        }
    }
}
