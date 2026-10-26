using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using BLL.Email;
using Microsoft.Extensions.Options;

namespace NewsArchive.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailOptions options;
        private readonly ILogger<SmtpEmailSender> logger;

        public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
        {
            this.options = options.Value;
            this.logger = logger;
        }

        public async Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            var smtp = options.Smtp;
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

            using var client = new SmtpClient(smtp.Host, smtp.Port)
            {
                EnableSsl = smtp.UseSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = Math.Clamp(smtp.TimeoutSeconds, 5, 120) * 1000
            };
            if (!string.IsNullOrEmpty(smtp.UserName))
                client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);

            try
            {
                await client.SendMailAsync(mail, cancellationToken);
                return true;
            }
            catch (Exception ex) when (ex is SmtpException or InvalidOperationException or FormatException)
            {
                logger.LogError(ex, "Email ({Category}) could not be sent", message.Category ?? "general");
                return false;
            }
        }
    }
}
