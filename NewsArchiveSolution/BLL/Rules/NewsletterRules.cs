using System.Net.Mail;

namespace BLL
{
    public static class NewsletterRules
    {
        public const int MaxEmailLength = 150;
        public const int DigestStories = 6;
        public const int DigestDays = 7;
        public static readonly TimeSpan ConfirmWindow = TimeSpan.FromHours(48);
        public static readonly TimeSpan ResendDelay = TimeSpan.FromMinutes(2);
        public static readonly TimeSpan PendingRetention = TimeSpan.FromDays(7);

        public static string? NormalizeEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var trimmed = email.Trim().ToLowerInvariant();
            if (trimmed.Length > MaxEmailLength || trimmed.Contains(' ')) return null;
            try
            {
                var address = new MailAddress(trimmed);
                var at = address.Address.LastIndexOf('@');
                return address.Address == trimmed && at > 0 && address.Address.IndexOf('.', at) > at + 1 ? trimmed : null;
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }

    public enum SubscribeStatus
    {
        ConfirmationNeeded,
        AlreadySubscribed,
        TooSoon,
        InvalidEmail
    }

    public enum ConfirmStatus
    {
        Confirmed,
        AlreadyConfirmed,
        Expired,
        NotFound
    }
}
