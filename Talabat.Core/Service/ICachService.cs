using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Service
{
    public interface ICachService
    {

        public Task CachRepoAsync (string key, object value, TimeSpan expiration );

        public Task <string> GetCachAsync(string key);

    }
}
