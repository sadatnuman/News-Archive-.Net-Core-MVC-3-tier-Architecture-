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

        public static string Article(int id, string? slug) =>
            string.IsNullOrEmpty(slug) ? $"/news/{id}" : $"/news/{id}/{Uri.EscapeDataString(slug)}";

        public static string Category(int id, string? slug) =>
            string.IsNullOrEmpty(slug) ? $"/category/{id}" : $"/category/{id}/{Uri.EscapeDataString(slug)}";

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
