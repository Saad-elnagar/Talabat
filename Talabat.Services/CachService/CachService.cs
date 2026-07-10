using Microsoft.EntityFrameworkCore.Storage;
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
    public class CachService : ICachService
    {
        private readonly StackExchange.Redis.IDatabase _database;
        public CachService(IConnectionMultiplexer redis)
        {
            _database = redis.GetDatabase();
        }
       public async Task CachRepoAsync(string key, object response, TimeSpan expiration)
        {

            if (response == null) return ;           
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            // Serialize the response object to a JSON string
            var serializedResponse = JsonSerializer.Serialize(response, options);
              
            await _database.StringSetAsync(key, serializedResponse, expiration);

        }

        public async Task<string> GetCachAsync(string key)
        {
            var cachedResponse = await _database.StringGetAsync(key);
            if (!cachedResponse.IsNullOrEmpty)
            {
                return cachedResponse!;
            }
            return string.Empty;
        }
    }
}
