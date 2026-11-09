namespace BLL.Email
{
    public interface IEmailSender
    {
        bool IsEnabled { get; }

        Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
    }

    public record EmailMessage(string To, string Subject, string TextBody, string? HtmlBody = null, string? Category = null, string? UnsubscribeUrl = null);
}
