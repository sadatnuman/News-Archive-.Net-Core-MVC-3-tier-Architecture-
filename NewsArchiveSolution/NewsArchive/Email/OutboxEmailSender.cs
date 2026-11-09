using System.Globalization;
using System.Text;
using BLL.Email;
using Microsoft.Extensions.Options;

namespace NewsArchive.Email
{
    public sealed class OutboxEmailSender : EmailSender
    {
        private readonly EmailOptions options;
        private readonly string? folder;

        public OutboxEmailSender(IOptions<EmailOptions> options, IWebHostEnvironment environment, ILogger<OutboxEmailSender> logger)
            : base(logger)
        {
            this.options = options.Value;
            folder = environment.IsDevelopment()
                ? Path.GetFullPath(Path.Combine(environment.ContentRootPath, this.options.OutboxFolder))
                : null;
        }

        protected override IMailTransport CreateTransport() => new OutboxTransport(folder, options);
    }

    public sealed class OutboxTransport : IMailTransport
    {
        private readonly string? folder;
        private readonly EmailOptions options;

        public OutboxTransport(string? folder, EmailOptions options)
        {
            this.folder = folder;
            this.options = options;
        }

        public async Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            if (folder == null) return;

            Directory.CreateDirectory(folder);
            var name = $"{DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff", CultureInfo.InvariantCulture)}-{Safe(message.Category ?? "mail")}-{Guid.NewGuid():N}.txt";
            var content = new StringBuilder()
                .Append("From: ").Append(options.FromName).Append(" <").Append(options.FromAddress).AppendLine(">")
                .Append("To: ").AppendLine(message.To)
                .Append("Subject: ").AppendLine(message.Subject)
                .AppendLine(message.UnsubscribeUrl == null ? "" : "List-Unsubscribe: <" + message.UnsubscribeUrl + ">")
                .AppendLine()
                .AppendLine(message.TextBody)
                .ToString();
            await File.WriteAllTextAsync(Path.Combine(folder, name), content, Encoding.UTF8, cancellationToken);
        }

        public void Dispose()
        {
        }

        private static string Safe(string value) => new(value.Where(char.IsLetterOrDigit).Take(20).ToArray());
    }
}
