namespace Talabat.Controllers
{
    public class CacheKeyController : BaseController
    {
        public IMemoryCache _memoryCache;


        public CacheKeyController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }
        [HttpGet("live-keys")]
        public IActionResult GetMemoryCacheKeys()
        {


            var coherentStateField = _memoryCache.GetType()
                    .GetField("_coherentState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var coherentState = coherentStateField?.GetValue(_memoryCache);


            var entriesCollectionProp = coherentState?.GetType()
                .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var entriesCollection = entriesCollectionProp?.GetValue(coherentState) as System.Collections.ICollection;

            var cacheKeys = new List<string>();

            if (entriesCollection != null)
            {
                foreach (var item in entriesCollection)
                {
                    var keyProperty = item.GetType().GetProperty("Key");
                    var key = keyProperty?.GetValue(item);
                    if (key != null) cacheKeys.Add(key.ToString()!);
                }
            }

            return Ok(cacheKeys);
        }
    }
}
