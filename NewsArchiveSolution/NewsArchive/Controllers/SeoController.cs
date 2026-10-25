using BLL;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NewsArchive.Seo;
using System.Text;
using System.Xml.Linq;

namespace NewsArchive.Controllers
{
    public class SeoController : AppController
    {
        private static readonly XNamespace SitemapNs = "http://www.sitemaps.org/schemas/sitemap/0.9";
        private static readonly XNamespace NewsNs = "http://www.google.com/schemas/sitemap-news/0.9";
        private static readonly XNamespace ImageNs = "http://www.google.com/schemas/sitemap-image/1.1";

        private readonly ArticleService articleService;
        private readonly CategoryService categoryService;
        private readonly TagService tagService;
        private readonly AuthorService authorService;
        private readonly SeoUrls urls;
        private readonly SiteOptions site;

        public SeoController(ArticleService articleService, CategoryService categoryService, TagService tagService, AuthorService authorService, SeoUrls urls, IOptions<SiteOptions> site)
        {
            this.authorService = authorService;
            this.tagService = tagService;
            this.articleService = articleService;
            this.categoryService = categoryService;
            this.urls = urls;
            this.site = site.Value;
        }

        [HttpGet("robots.txt")]
        [ResponseCache(Duration = 86400)]
        public ContentResult Robots()
        {
            var sb = new StringBuilder();
            sb.AppendLine("User-agent: *");
            sb.AppendLine("Allow: /");
            sb.AppendLine("Disallow: /Admin/");
            sb.AppendLine("Disallow: /Auth/");
            sb.AppendLine("Disallow: /Member/");
            sb.AppendLine("Disallow: /Article/Create");
            sb.AppendLine("Disallow: /Article/Edit/");
            sb.AppendLine("Disallow: /Article/Delete/");
            sb.AppendLine("Disallow: /Article/MyArticles");
            sb.AppendLine("Disallow: /search?");
            sb.AppendLine("Disallow: /api/");
            sb.AppendLine("Disallow: /swagger/");
            sb.AppendLine();
            sb.AppendLine($"Sitemap: {urls.Absolute("/sitemap.xml")}");
            sb.AppendLine($"Sitemap: {urls.Absolute("/news-sitemap.xml")}");
            return Content(sb.ToString(), "text/plain", Encoding.UTF8);
        }

        [HttpGet("sitemap.xml")]
        [ResponseCache(Duration = 3600)]
        public ContentResult Sitemap()
        {
            var articles = articleService.GetPublishedForSitemap();
            var root = new XElement(SitemapNs + "urlset",
                new XAttribute(XNamespace.Xmlns + "image", ImageNs));

            void AddUrl(string path, DateTime? modified, string changeFreq, string priority, XElement? image = null)
            {
                var url = new XElement(SitemapNs + "url",
                    new XElement(SitemapNs + "loc", urls.Absolute(path)));
                if (modified.HasValue)
                    url.Add(new XElement(SitemapNs + "lastmod", modified.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")));
                url.Add(new XElement(SitemapNs + "changefreq", changeFreq));
                url.Add(new XElement(SitemapNs + "priority", priority));
                if (image != null) url.Add(image);
                root.Add(url);
            }

            var latest = articles.Count > 0 ? articles.Max(a => a.UpdatedAt ?? a.PublishDate) : (DateTime?)null;
            var categories = categoryService.Get();
            foreach (var lang in AppLanguages.All)
            {
                AddUrl(SeoUrls.Page("/", lang), latest, "hourly", "1.0");
                AddUrl(SeoUrls.Page("/trending", lang), latest, "hourly", "0.8");
                AddUrl(SeoUrls.Page("/breaking-news", lang), latest, "hourly", "0.8");
                AddUrl(SeoUrls.Page("/Category", lang), null, "weekly", "0.6");
                foreach (var c in categories)
                    AddUrl(SeoUrls.Category(c.CategoryId, c.Slug, lang), null, "daily", "0.7");
                AddUrl(SeoUrls.Page("/tags", lang), null, "weekly", "0.5");
                AddUrl(SeoUrls.Page("/archive", lang), latest, "daily", "0.5");
                AddUrl(SeoUrls.Page("/authors", lang), null, "weekly", "0.4");
                foreach (var a in authorService.GetAuthors(lang))
                    AddUrl(SeoUrls.Author(a.UserId, a.Name, lang), null, "weekly", "0.5");
                var months = articleService.GetArchiveMonths(lang);
                foreach (var year in months.Select(m => m.Year).Distinct())
                    AddUrl(SeoUrls.Archive(year, language: lang), null, "monthly", "0.4");
                foreach (var m in months)
                    AddUrl(SeoUrls.Archive(m.Year, m.Month, language: lang), null, "weekly", "0.4");
                foreach (var t in tagService.GetWithCounts(lang).Where(t => t.ArticleCount > 0))
                    AddUrl(SeoUrls.Tag(t.TagId, t.Slug, lang), null, "daily", "0.6");
            }

            foreach (var a in articles)
            {
                XElement? image = null;
                if (!string.IsNullOrEmpty(a.ImagePath))
                    image = new XElement(ImageNs + "image",
                        new XElement(ImageNs + "loc", urls.AbsoluteImage(a.ImagePath)));
                AddUrl(SeoUrls.Article(a), a.UpdatedAt ?? a.PublishDate, "weekly", "0.9", image);
            }

            foreach (var lang in AppLanguages.All)
                foreach (var page in new[] { "/about", "/contact", "/editorial-policy", "/privacy", "/terms" })
                    AddUrl(SeoUrls.Page(page, lang), null, "yearly", "0.3");

            return Xml(root);
        }

        [HttpGet("news-sitemap.xml")]
        [ResponseCache(Duration = 900)]
        public ContentResult NewsSitemap()
        {
            var cutoff = DateTime.Now.AddDays(-2);
            var root = new XElement(SitemapNs + "urlset",
                new XAttribute(XNamespace.Xmlns + "news", NewsNs));

            foreach (var a in articleService.GetPublishedForSitemap().Where(a => a.PublishDate >= cutoff).Take(1000))
            {
                root.Add(new XElement(SitemapNs + "url",
                    new XElement(SitemapNs + "loc", urls.Absolute(SeoUrls.Article(a))),
                    new XElement(NewsNs + "news",
                        new XElement(NewsNs + "publication",
                            new XElement(NewsNs + "name", site.Name),
                            new XElement(NewsNs + "language", a.LanguageCode)),
                        new XElement(NewsNs + "publication_date", a.PublishDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")),
                        new XElement(NewsNs + "title", a.Title))));
            }

            return Xml(root);
        }

        [HttpGet("rss.xml")]
        [ResponseCache(Duration = 900)]
        public ContentResult Rss()
        {
            XNamespace atom = "http://www.w3.org/2005/Atom";
            var language = UiLanguage.Current;
            var channel = new XElement("channel",
                new XElement(atom + "link",
                    new XAttribute("href", urls.Absolute(SeoUrls.Page("/rss.xml", language))),
                    new XAttribute("rel", "self"),
                    new XAttribute("type", "application/rss+xml")),
                new XElement("title", site.Name),
                new XElement("link", urls.Absolute(SeoUrls.Page("/", language))),
                new XElement("description", L[site.Description].Value),
                new XElement("language", language),
                new XElement("lastBuildDate", DateTime.Now.ToString("R")));

            foreach (var a in articleService.GetLatest(50, language))
            {
                var link = urls.Absolute(SeoUrls.Article(a));
                var item = new XElement("item",
                    new XElement("title", a.Title),
                    new XElement("link", link),
                    new XElement("guid", new XAttribute("isPermaLink", "true"), link),
                    new XElement("pubDate", a.PublishDate.ToString("R")),
                    new XElement("description", a.Summary));
                if (!string.IsNullOrEmpty(a.ImagePath))
                    item.Add(new XElement("enclosure",
                        new XAttribute("url", urls.AbsoluteImage(a.ImagePath)),
                        new XAttribute("type", ImageType(a.ImagePath)),
                        new XAttribute("length", 0)));
                channel.Add(item);
            }

            var rss = new XElement("rss",
                new XAttribute("version", "2.0"),
                new XAttribute(XNamespace.Xmlns + "atom", atom),
                channel);
            return Xml(rss, "application/rss+xml");
        }

        private static string ImageType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "image/jpeg"
        };

        private ContentResult Xml(XElement root, string contentType = "application/xml")
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
            return Content(doc.Declaration + Environment.NewLine + doc.Root, contentType, Encoding.UTF8);
        }
    }
}
