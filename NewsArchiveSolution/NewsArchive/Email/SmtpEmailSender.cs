using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using BLL.Email;
using Microsoft.Extensions.Options;

namespace NewsArchive.Email
{
    public sealed class SmtpEmailSender : EmailSender
    {
        private readonly EmailOptions options;

        public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
            : base(logger)
        {
            this.options = options.Value;
        }

        protected override IMailTransport CreateTransport() => new SmtpTransport(options);
    }

    public sealed class SmtpTransport : IMailTransport
    {
        private readonly EmailOptions options;
        private readonly SmtpClient client;

        public SmtpTransport(EmailOptions options)
        {
            this.options = options;
            var smtp = options.Smtp;
            client = new SmtpClient(smtp.Host, smtp.Port)
            {
                EnableSsl = smtp.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = Math.Clamp(smtp.TimeoutSeconds, 5, 120) * 1000
            };
            if (!string.IsNullOrEmpty(smtp.UserName))
                client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);
        }

        public async Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            using var mail = new MailMessage
            {
                From = new MailAddress(options.FromAddress, options.FromName, Encoding.UTF8),
                Subject = message.Subject,
                SubjectEncoding = Encoding.UTF8,
                Body = message.TextBody,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = false
            };
            mail.To.Add(new MailAddress(message.To));
            if (message.HtmlBody != null)
                mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.HtmlBody, Encoding.UTF8, MediaTypeNames.Text.Html));
            if (message.UnsubscribeUrl != null)
            {
                mail.Headers.Add("List-Unsubscribe", $"<{message.UnsubscribeUrl}>");
                mail.Headers.Add("List-Unsubscribe-Post", "List-Unsubscribe=One-Click");
            }

            await client.SendMailAsync(mail, cancellationToken);
        }

        public void Dispose() => client.Dispose();
    }
}
