using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Html;

namespace NewsArchive.Content
{
    public static class ArticleRenderer
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .DisableHtml()
            .UseSoftlineBreakAsHardlineBreak()
            .UseEmphasisExtras()
            .UseAutoLinks()
            .Build();

        public static IHtmlContent Render(string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return HtmlString.Empty;

            var document = Markdown.Parse(markdown.Replace("\r\n", "\n"), Pipeline);

            foreach (var heading in document.Descendants<HeadingBlock>())
                heading.Level = Math.Clamp(heading.Level + 1, 2, 6);

            foreach (var link in document.Descendants<LinkInline>())
            {
                if (link.IsImage)
                {
                    if (!IsSafeImage(link.Url)) link.Url = string.Empty;
                    link.GetAttributes().AddPropertyIfNotExist("loading", "lazy");
                    continue;
                }

                if (!IsSafeLink(link.Url))
                {
                    link.Url = "#";
                    continue;
                }

                if (IsExternal(link.Url))
                    link.GetAttributes().AddPropertyIfNotExist("rel", "nofollow noopener noreferrer");
            }

            foreach (var link in document.Descendants<AutolinkInline>())
            {
                if (!IsSafeLink(link.Url)) link.Url = "#";
            }

            using var writer = new StringWriter();
            var renderer = new HtmlRenderer(writer);
            Pipeline.Setup(renderer);
            renderer.Render(document);
            writer.Flush();
            return new HtmlString(writer.ToString());
        }

        private static bool IsSitePath(string url) =>
            url.Length > 0 && url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'));

        public static bool IsSafeLink(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            url = url.Trim();
            if (url.Contains('\\')) return false;
            if (url.StartsWith('#') || IsSitePath(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeMailto);
        }

        private static bool IsSafeImage(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            url = url.Trim();
            if (url.Contains('\\')) return false;
            if (IsSitePath(url)) return true;
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
        }

        private static bool IsExternal(string? url)
        {
            return url != null && Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme != Uri.UriSchemeMailto;
        }
    }
}
