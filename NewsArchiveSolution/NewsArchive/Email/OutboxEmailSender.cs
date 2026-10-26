using System.Text;
using BLL.Email;
using Microsoft.Extensions.Options;

namespace NewsArchive.Email
{
    public class OutboxEmailSender : IEmailSender
    {
        private readonly EmailOptions options;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<OutboxEmailSender> logger;

        public OutboxEmailSender(IOptions<EmailOptions> options, IWebHostEnvironment environment, ILogger<OutboxEmailSender> logger)
        {
            this.options = options.Value;
            this.environment = environment;
            this.logger = logger;
        }

        public async Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Email ({Category}) queued for {Recipient}: {Subject}", message.Category ?? "general", Mask(message.To), message.Subject);
            if (!environment.IsDevelopment()) return true;

            var folder = Path.GetFullPath(Path.Combine(environment.ContentRootPath, options.OutboxFolder));
            Directory.CreateDirectory(folder);
            var name = $"{DateTime.Now:yyyyMMdd-HHmmss-fff}-{Safe(message.Category ?? "mail")}-{Guid.NewGuid():N}.txt";
            var content = new StringBuilder()
                .AppendLine($"From: {options.FromName} <{options.FromAddress}>")
                .AppendLine($"To: {message.To}")
                .AppendLine($"Subject: {message.Subject}")
                .AppendLine(message.UnsubscribeUrl == null ? "" : $"List-Unsubscribe: <{message.UnsubscribeUrl}>")
                .AppendLine()
                .AppendLine(message.TextBody)
                .ToString();
            await File.WriteAllTextAsync(Path.Combine(folder, name), content, Encoding.UTF8, cancellationToken);
            return true;
        }

        private static string Safe(string value) => new(value.Where(char.IsLetterOrDigit).Take(20).ToArray());

        private static string Mask(string email)
        {
            var at = email.IndexOf('@');
            return at <= 1 ? "***" : email[0] + "***" + email[at..];
        }
    }
}
