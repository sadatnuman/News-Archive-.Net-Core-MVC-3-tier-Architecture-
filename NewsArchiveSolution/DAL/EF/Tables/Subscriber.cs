namespace DAL.EF.Tables;

public partial class Subscriber
{
    public int SubscriberId { get; set; }

    public string Email { get; set; } = null!;

    public string LanguageCode { get; set; } = LanguageCodes.English;

    public string ConfirmTokenHash { get; set; } = null!;

    public string UnsubscribeToken { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

    public DateTime? UnsubscribedAt { get; set; }

    public DateTime? LastSentAt { get; set; }

    public virtual Language Language { get; set; } = null!;
}
