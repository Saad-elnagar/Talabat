using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Talabat.Application.Abstractions;

namespace Talabat.Infrastructure.Services.Cache
{
    public class MultiLayerCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly StackExchange.Redis.IDatabase _redisDb;
        private readonly ConcurrentDictionary<string, byte> _trackedKeys = new();
        private static readonly SemaphoreSlim _cacheLock = new SemaphoreSlim(1, 1);
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

            // L1
            SetMemoryCache(key, json, TimeSpan.FromMinutes(Math.Min(expiration.TotalMinutes, 5)));
            Console.WriteLine("set cach in Memory");

            // L2
            await _redisDb.StringSetAsync(key, json, expiration);
            Console.WriteLine("set cach in Redis");

        }

        public async Task<string> GetCachedResponseAsync(string key)
        {

            // L1: Memory
            if (_memoryCache.TryGetValue(key, out string? cached) && !string.IsNullOrEmpty(cached))
            {
                Console.WriteLine("Catch fROM Memory");
                return cached;
            }
            await _cacheLock.WaitAsync();
            try
            {
                // L2: Redis
                var redisValue = await _redisDb.StringGetAsync(key);
                if (!redisValue.IsNullOrEmpty)
                {
                    SetMemoryCache(key, (string)redisValue!, TimeSpan.FromMinutes(5));
                    Console.WriteLine("Catch fROM Redis");
                    return redisValue!;
                }

                return string.Empty;
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        public IReadOnlyCollection<string> GetLiveKeys()
        {
            var liveKeys = new List<string>();

            foreach (var key in _trackedKeys.Keys)
            {
                if (_memoryCache.TryGetValue(key, out _))
                {
                    liveKeys.Add(key);
                }
                else
                {
                    _trackedKeys.TryRemove(key, out _);
                }
            }

            return liveKeys;
        }

        private void SetMemoryCache(string key, string value, TimeSpan expiration)
        {
            _memoryCache.Set(key, value, expiration);
            _trackedKeys[key] = 0;
        }

    }
}
