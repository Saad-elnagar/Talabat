using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Text.Json;
using Talabat.Core.Service;

namespace Talabat.Services.CachService
{
    public class MultiLayerCacheService : ICachService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IDatabase _redisDb;

        // Per-key locks so concurrent requests for *different* keys never block each other —
        // only requests racing for the *same* key are serialized.
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public MultiLayerCacheService(IMemoryCache memoryCache, IConnectionMultiplexer redisDb)
        {
            _memoryCache = memoryCache;
            _redisDb = redisDb.GetDatabase();
        }

        public async Task CacheResponseAsync(string key, object response, TimeSpan expiration)
        {
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // L1: Memory Cache
            var memoryExpiry = TimeSpan.FromMinutes(Math.Min(expiration.TotalMinutes, 5));
            _memoryCache.Set(key, json, memoryExpiry);
            Console.WriteLine($" Set cache in Memory: {key}");

            // L2: Redis Cache
            await _redisDb.StringSetAsync(key, json, expiration);
            Console.WriteLine($" Set cache in Redis: {key}");
        }

        public async Task<string?> GetCachedResponseAsync(string key)
        {
            // L1: Memory
            if (_memoryCache.TryGetValue(key, out string? cached) && !string.IsNullOrEmpty(cached))
            {
                Console.WriteLine($" Cache HIT (Memory): {key}");
                return cached;
            }
            Console.WriteLine($" Cache MISS (Memory): {key}");

            // Serialize concurrent lookups for the *same* key so a burst of simultaneous
            // requests doesn't all hit Redis at once (a "thundering herd" for that one key).
            // Requests for other keys are unaffected since each key gets its own lock.
            var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            if (!await lockObj.WaitAsync(TimeSpan.FromSeconds(5)))
                throw new TimeoutException($"Cache timeout for key: {key}");

            // BUG FIX: the previous version acquired this lock but never released it (no
            // try/finally), so after the first caller for any given key, every later request
            // for that same key would hang forever waiting on a lock nobody frees. The
            // try/finally below guarantees release even if Redis throws.
            try
            {
                // Re-check memory cache: another request may have already populated it while
                // we were waiting for the lock — no need to hit Redis again in that case.
                if (_memoryCache.TryGetValue(key, out string? cachedAfterLock) && !string.IsNullOrEmpty(cachedAfterLock))
                {
                    Console.WriteLine($" Cache HIT (Memory after lock): {key}");
                    return cachedAfterLock;
                }

                // L2: Redis
                var redisValue = await _redisDb.StringGetAsync(key);
                if (!redisValue.IsNullOrEmpty)
                {
                    _memoryCache.Set(key, (string)redisValue!, TimeSpan.FromMinutes(5));
                    Console.WriteLine($" Cache HIT (Redis): {key}");
                    return redisValue!;
                }

                Console.WriteLine($" Cache MISS (Final): {key} - Value not found");
                return null;
            }
            finally
            {
                lockObj.Release();

                // Clean up the per-key semaphore once nobody else is waiting on it, so the
                // _locks dictionary doesn't grow forever as new cache keys get created.
                if (lockObj.CurrentCount == 1)
                {
                    _locks.TryRemove(key, out _);
                }
            }
        }
    }
}
