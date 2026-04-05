using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using PCShop_Backend.Dtos;

namespace PCShop_Backend.Service
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        public CacheService(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var cachedData = await _distributedCache.GetStringAsync(key);
            return cachedData == null ? default : JsonConvert.DeserializeObject<T>(cachedData);
        }

        public async Task RemoveAsync(string key)
        {
            await _distributedCache.RemoveAsync(key);
        }

        public async Task SetAsync<T>(string key, T value)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };
            
            var jsonData = JsonConvert.SerializeObject(value);

            await _distributedCache.SetStringAsync(key, jsonData, options);
        }
    }
}
