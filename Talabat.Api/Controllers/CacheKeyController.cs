using Microsoft.AspNetCore.Mvc;
using Talabat.Application.Abstractions;

namespace Talabat.Api.Controllers
{
    public class CacheKeyController : BaseController
    {
        private readonly ICacheService _cacheService;

        public CacheKeyController(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        [HttpGet("live-keys")]
        public IActionResult GetMemoryCacheKeys()
        {
            return Ok(_cacheService.GetLiveKeys());
        }
    }
}
