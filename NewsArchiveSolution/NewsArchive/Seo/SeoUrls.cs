using Microsoft.Extensions.Options;

namespace NewsArchive.Seo
{
    public class SeoUrls
    {
        private readonly SiteOptions site;
        private readonly IHttpContextAccessor accessor;

        public SeoUrls(IOptions<SiteOptions> site, IHttpContextAccessor accessor)
        {
            this.site = site.Value;
            this.accessor = accessor;
        }

        public string BaseUrl
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(site.BaseUrl))
                    return site.BaseUrl.TrimEnd('/');

                var request = accessor.HttpContext?.Request;
                return request == null ? string.Empty : $"{request.Scheme}://{request.Host}{request.PathBase}";
            }
        }

        public static string ArticlePath(int id, string? slug) =>
            string.IsNullOrEmpty(slug) ? $"/news/{id}" : $"/news/{id}/{Uri.EscapeDataString(slug)}";

        public static string Article(BLL.DTOs.ArticleDTO article) =>
            UiLanguage.WithLanguage(ArticlePath(article.ArticleId, article.Slug), article.LanguageCode);

        public static string CategoryPath(int id, string? slug) =>
            string.IsNullOrEmpty(slug) ? $"/category/{id}" : $"/category/{id}/{Uri.EscapeDataString(slug)}";

        public static string Category(int id, string? slug, string? language = null) =>
            UiLanguage.WithLanguage(CategoryPath(id, slug), language ?? UiLanguage.Current);

        public static string TagPath(int id, string? slug) =>
            string.IsNullOrEmpty(slug) ? $"/tag/{id}" : $"/tag/{id}/{Uri.EscapeDataString(slug)}";

        public static string Tag(int id, string? slug, string? language = null, int page = 1) =>
            UiLanguage.WithLanguage(TagPath(id, slug) + (page > 1 ? $"?page={page}" : string.Empty), language ?? UiLanguage.Current);

        public static string ArchivePath(int? year = null, int? month = null, int? day = null)
        {
            if (year == null) return "/archive";
            if (month == null) return $"/archive/{year}";
            return day == null ? $"/archive/{year}/{month:D2}" : $"/archive/{year}/{month:D2}/{day:D2}";
        }

        public static string Archive(int? year = null, int? month = null, int? day = null, string? language = null, int page = 1) =>
            UiLanguage.WithLanguage(ArchivePath(year, month, day) + (page > 1 ? $"?page={page}" : string.Empty), language ?? UiLanguage.Current);

        public static string Page(string path, string? language = null) =>
            UiLanguage.WithLanguage(path, language ?? UiLanguage.Current);

        public static string? Image(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            if (path.StartsWith("http://") || path.StartsWith("https://")) return path;

            var trimmed = path.TrimStart('~').TrimStart('/');
            return "/" + string.Join('/', trimmed.Split('/').Select(Uri.EscapeDataString));
        }

        public string Absolute(string? path)
        {
            if (string.IsNullOrEmpty(path)) return BaseUrl + "/";
            if (path.StartsWith("http://") || path.StartsWith("https://")) return path;
            return BaseUrl + (path.StartsWith('/') ? path : "/" + path);
        }

        public string AbsoluteImage(string? path) => Absolute(Image(path) ?? site.DefaultImage);
    }
}
