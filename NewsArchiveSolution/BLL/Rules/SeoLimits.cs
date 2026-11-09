namespace BLL
{
    public static class SeoLimits
    {
        public const int MaxSitemapArticles = 45000;
        public const int MaxNewsSitemapArticles = 1000;
        public static readonly TimeSpan NewsSitemapWindow = TimeSpan.FromDays(2);
    }
}
