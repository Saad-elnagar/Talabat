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

        // lock for thread safety when accessing the cache
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

            if (_memoryCache.TryGetValue(key, out string? cached) && !string.IsNullOrEmpty(cached))
            {
                Console.WriteLine($" Cache HIT (Memory): {key}");
                return cached;
            }
            Console.WriteLine($"Cache MISS (Memory): {key}");


            var redisValue = await _redisDb.StringGetAsync(key);
            if (!redisValue.IsNullOrEmpty)
            {

                _memoryCache.Set(key, (string)redisValue!, TimeSpan.FromMinutes(5));
                Console.WriteLine($" Cache HIT (Redis): {key}");
                return redisValue!;
            }
            Console.WriteLine($" Cache MISS (Redis): {key}");




            var lockObj = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));


            if (!await lockObj.WaitAsync(TimeSpan.FromSeconds(5)))
            {
                throw new TimeoutException($"Cache timeout for key: {key}");
            }

            try
            {



                if (_memoryCache.TryGetValue(key, out string? cachedAfterLock) && !string.IsNullOrEmpty(cachedAfterLock))
                {
                    Console.WriteLine($" Cache HIT (Memory after lock): {key}");
                    return cachedAfterLock;
                }


                var redisValueAfterLock = await _redisDb.StringGetAsync(key);
                if (!redisValueAfterLock.IsNullOrEmpty)
                {
                    _memoryCache.Set(key, (string)redisValueAfterLock!, TimeSpan.FromMinutes(5));
                    Console.WriteLine($" Cache HIT (Redis after lock): {key}");
                    return redisValueAfterLock!;
                }


                Console.WriteLine($" Cache MISS (Final): {key} - Value not found");
                return null;
            }
            finally
            {

                lockObj.Release();
                lockObj.Release();
                if (lockObj.CurrentCount == 1)
                {
                    _locks.TryRemove(key, out _);
                }
            }
        }
    }
}