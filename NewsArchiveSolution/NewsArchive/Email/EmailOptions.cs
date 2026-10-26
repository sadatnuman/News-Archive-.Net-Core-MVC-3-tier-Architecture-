namespace NewsArchive.Email
{
    public class EmailOptions
    {
        public const string SectionName = "Email";

        public string FromAddress { get; set; } = "no-reply@meridiannewsfolio.local";

        public string FromName { get; set; } = "The Meridian Newsfolio";

        public string OutboxFolder { get; set; } = "App_Data/outbox";

        public SmtpOptions Smtp { get; set; } = new();

        public bool UseSmtp => !string.IsNullOrWhiteSpace(Smtp.Host);
    }

    public class SmtpOptions
    {
        public string? Host { get; set; }

        public int Port { get; set; } = 587;

        public bool UseSsl { get; set; } = true;

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public int TimeoutSeconds { get; set; } = 30;
    }
}
