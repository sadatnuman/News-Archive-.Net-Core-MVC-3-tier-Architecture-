using System.Collections.Concurrent;
using BLL.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace NewsArchive.Caching
{
    public class MemoryAppCache : IAppCache
    {
        private readonly IMemoryCache cache;
        private readonly ConcurrentDictionary<string, long> versions = new();

        public MemoryAppCache(IMemoryCache cache)
        {
            this.cache = cache;
        }

        private string FullKey(string region, string key) => $"app:{region}:{versions.GetOrAdd(region, 0)}:{key}";

        public T GetOrCreate<T>(string region, string key, TimeSpan lifetime, Func<T> factory)
        {
            return cache.GetOrCreate(FullKey(region, key), entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = lifetime;
                return factory();
            })!;
        }

        public void Invalidate(string region)
        {
            versions.AddOrUpdate(region, 1, (_, current) => current + 1);
        }
    }
}
