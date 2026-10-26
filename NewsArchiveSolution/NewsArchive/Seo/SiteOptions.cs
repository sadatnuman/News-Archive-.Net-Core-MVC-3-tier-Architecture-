namespace NewsArchive.Seo
{
    public class SiteOptions
    {
        public string Name { get; set; } = "The Meridian Newsfolio";

        public string Tagline { get; set; } = "News Preserved. Stories Rediscovered.";

        public string Description { get; set; } = "The Meridian Newsfolio publishes reviewed news from Bangladesh and around the world, in English and Bangla, and keeps every story in a searchable archive.";

        public string BaseUrl { get; set; } = string.Empty;

        public string DefaultImage { get; set; } = "/images/brand/og-default.png";

        public string Logo { get; set; } = "/images/brand/icon-512.png";

        public string Locale { get; set; } = "en_US";

        public string Language { get; set; } = "en";

        public string? TwitterHandle { get; set; }

        public string? ContactEmail { get; set; }

        public string[] SocialProfiles { get; set; } = Array.Empty<string>();

        public bool ShowDemoNotice { get; set; }

        public DemoAccount[] DemoAccounts { get; set; } = Array.Empty<DemoAccount>();
    }

    public class DemoAccount
    {
        public string Role { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
