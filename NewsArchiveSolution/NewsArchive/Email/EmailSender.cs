using System.Net.Mail;
using BLL.Email;

namespace NewsArchive.Email
{
    public interface IMailTransport : IDisposable
    {
        Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken);
    }

    public abstract partial class EmailSender : IEmailSender
    {
        private readonly ILogger logger;

        protected EmailSender(ILogger logger)
        {
            this.logger = logger;
        }

        public bool IsEnabled => true;

        protected abstract IMailTransport CreateTransport();

        public async Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            var category = message.Category ?? "general";
            if (!MailAddress.TryCreate(message.To, out _))
            {
                LogRejected(category);
                return false;
            }

            try
            {
                using var transport = CreateTransport();
                await transport.DeliverAsync(message, cancellationToken);
            }
            catch (Exception ex) when (ex is SmtpException or IOException or UnauthorizedAccessException or InvalidOperationException or FormatException)
            {
                LogFailed(ex, category);
                return false;
            }

            if (logger.IsEnabled(LogLevel.Information))
                LogQueued(category, Mask(message.To), message.Subject);
            return true;
        }

        public static string Mask(string email)
        {
            var at = email.IndexOf('@');
            return at <= 1 ? "***" : email[0] + "***" + email[at..];
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Email ({Category}) queued for {Recipient}: {Subject}")]
        private partial void LogQueued(string category, string recipient, string subject);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Email ({Category}) not sent: invalid recipient address")]
        private partial void LogRejected(string category);

        [LoggerMessage(Level = LogLevel.Error, Message = "Email ({Category}) could not be sent")]
        private partial void LogFailed(Exception exception, string category);
    }
}
