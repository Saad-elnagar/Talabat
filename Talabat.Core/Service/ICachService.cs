using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Service
{
    public interface ICachService
    {
        Task CacheResponseAsync (string key, object response, TimeSpan expiration);
        Task<string> GetCachedResponseAsync(string key);
       
    }
}
