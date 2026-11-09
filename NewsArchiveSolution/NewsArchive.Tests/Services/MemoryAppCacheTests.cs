using Microsoft.Extensions.Caching.Memory;
using NewsArchive.Caching;

namespace NewsArchive.Tests.Services
{
    public class MemoryAppCacheTests
    {
        [Fact]
        public void A_list_built_while_content_changes_is_never_served_afterwards()
        {
            using var memory = new MemoryCache(new MemoryCacheOptions());
            var cache = new MemoryAppCache(memory);

            var first = cache.GetOrCreate("content", "trending", TimeSpan.FromMinutes(2), () =>
            {
                cache.Invalidate("content");
                return "list read before the story was withdrawn";
            });

            Assert.Equal("list read before the story was withdrawn", first);
            Assert.Equal("fresh list", cache.GetOrCreate("content", "trending", TimeSpan.FromMinutes(2), () => "fresh list"));
        }

        [Fact]
        public void Invalidating_one_region_keeps_the_other()
        {
            using var memory = new MemoryCache(new MemoryCacheOptions());
            var cache = new MemoryAppCache(memory);
            cache.GetOrCreate("sections", "all", TimeSpan.FromMinutes(30), () => "sections v1");
            cache.GetOrCreate("content", "trending", TimeSpan.FromMinutes(2), () => "trending v1");

            cache.Invalidate("content");

            Assert.Equal("sections v1", cache.GetOrCreate("sections", "all", TimeSpan.FromMinutes(30), () => "sections v2"));
            Assert.Equal("trending v2", cache.GetOrCreate("content", "trending", TimeSpan.FromMinutes(2), () => "trending v2"));
        }
    }
}
