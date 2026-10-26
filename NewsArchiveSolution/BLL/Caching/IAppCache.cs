namespace BLL.Caching
{
    public interface IAppCache
    {
        T GetOrCreate<T>(string region, string key, TimeSpan lifetime, Func<T> factory);

        void Invalidate(string region);
    }

    public static class CacheRegions
    {
        public const string Sections = "sections";
        public const string Content = "content";
    }

    public static class CacheTimes
    {
        public static readonly TimeSpan Sections = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan Listings = TimeSpan.FromMinutes(2);
    }

    public sealed class NoCache : IAppCache
    {
        public T GetOrCreate<T>(string region, string key, TimeSpan lifetime, Func<T> factory) => factory();

        public void Invalidate(string region)
        {
        }
    }
}
