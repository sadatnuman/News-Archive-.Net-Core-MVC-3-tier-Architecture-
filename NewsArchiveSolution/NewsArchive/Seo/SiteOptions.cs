namespace NewsArchive.Seo
{
    public class SiteOptions
    {
        public string Name { get; set; } = "NewsArchive";

        public string Tagline { get; set; } = "Breaking news, trusted reporting and a searchable archive";

        public string Description { get; set; } = "Latest news, breaking stories and an archive of reporting on technology, politics, business, sports and more.";

        public string BaseUrl { get; set; } = string.Empty;

        public string DefaultImage { get; set; } = "/images/brand/og-default.png";

        public string Logo { get; set; } = "/images/brand/icon-512.png";

        public string Locale { get; set; } = "en_US";

        public string Language { get; set; } = "en";

        public string? TwitterHandle { get; set; }

        public string? ContactEmail { get; set; }

        public string[] SocialProfiles { get; set; } = Array.Empty<string>();
    }
}
