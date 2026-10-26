namespace NewsArchive.Auth
{
    public class ApiOptions
    {
        public const string CorsPolicy = "ApiClients";
        public const string LoginRateLimit = "login";
        public const string CommentRateLimit = "comment";
        public const string ReportRateLimit = "report";
        public const string NewsletterRateLimit = "newsletter";

        public bool SwaggerEnabled { get; set; }

        public int TokenLifetimeHours { get; set; } = 12;

        public string[] AllowedOrigins { get; set; } = [];

        public TimeSpan TokenLifetime => TimeSpan.FromHours(TokenLifetimeHours is > 0 and <= 720 ? TokenLifetimeHours : 12);
    }
}
