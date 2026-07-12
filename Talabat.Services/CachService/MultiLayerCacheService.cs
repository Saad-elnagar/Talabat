using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Talabat.Core.Service;

namespace Talabat.Services.CachService
{
    public class MultiLayerCacheService : ICachService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly StackExchange.Redis.IDatabase _redisDb;

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
            _memoryCache.Set(key, json, TimeSpan.FromMinutes(Math.Min(expiration.TotalMinutes, 5)));
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

            // L2: Redis
            var redisValue = await _redisDb.StringGetAsync(key);
            if (!redisValue.IsNullOrEmpty)
            {
                _memoryCache.Set(key, (string)redisValue!, TimeSpan.FromMinutes(5));
                Console.WriteLine("Catch fROM Redis");
                return redisValue!;
            }

            return string.Empty;
        }

    }
}
