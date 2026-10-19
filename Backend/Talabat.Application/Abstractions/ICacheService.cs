using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Application.Abstractions
{
    public interface ICacheService
    {
        Task CacheResponseAsync (string key, object response, TimeSpan expiration);
        Task<string> GetCachedResponseAsync(string key);
        IReadOnlyCollection<string> GetLiveKeys();
    }
}
