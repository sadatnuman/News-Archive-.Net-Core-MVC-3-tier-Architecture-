namespace NewsArchive.Seo
{
    public class PageMeta
    {
        public string? Title { get; set; }

        public string? Description { get; set; }

        public string? CanonicalPath { get; set; }

        public string? Image { get; set; }

        public string? ImageAlt { get; set; }

        public string Type { get; set; } = "website";

        public bool NoIndex { get; set; }

        public DateTime? PublishedTime { get; set; }

        public DateTime? ModifiedTime { get; set; }

        public string? Section { get; set; }

        public string? Author { get; set; }

        public List<(string Name, string Path)> Breadcrumbs { get; set; } = new();

        public List<object> JsonLd { get; set; } = new();

        public static string Shorten(string? text, int max = 160)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var clean = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            if (clean.Length <= max) return clean;
            var cut = clean.LastIndexOf(' ', max - 1);
            return clean[..(cut > 60 ? cut : max - 1)].TrimEnd(',', '.', ';', ':') + "…";
        }
    }
}
