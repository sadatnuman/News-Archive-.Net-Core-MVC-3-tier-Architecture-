namespace BLL.DTOs
{
    public class SubscriberDTO
    {
        public int SubscriberId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string LanguageCode { get; set; } = AppLanguages.English;

        public DateTime CreatedAt { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        public DateTime? UnsubscribedAt { get; set; }

        public DateTime? LastSentAt { get; set; }

        public string Status => UnsubscribedAt != null ? "Unsubscribed" : ConfirmedAt != null ? "Active" : "Waiting for confirmation";
    }

    public class SubscribeResultDTO
    {
        public SubscribeStatus Status { get; set; }

        public string Email { get; set; } = string.Empty;

        public string LanguageCode { get; set; } = AppLanguages.English;

        public string? ConfirmToken { get; set; }

        public string? UnsubscribeToken { get; set; }
    }

    public class ConfirmResultDTO
    {
        public ConfirmStatus Status { get; set; }

        public string? Email { get; set; }

        public string LanguageCode { get; set; } = AppLanguages.English;

        public string? UnsubscribeToken { get; set; }
    }

    public class NewsletterRecipientDTO
    {
        public int SubscriberId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string UnsubscribeToken { get; set; } = string.Empty;
    }

    public class NewsletterDigestDTO
    {
        public string LanguageCode { get; set; } = AppLanguages.English;

        public List<ArticleDTO> Stories { get; set; } = new();

        public List<NewsletterRecipientDTO> Recipients { get; set; } = new();
    }

    public class NewsletterStatsDTO
    {
        public int ActiveEnglish { get; set; }

        public int ActiveBangla { get; set; }

        public int Pending { get; set; }

        public int Active => ActiveEnglish + ActiveBangla;
    }
}
