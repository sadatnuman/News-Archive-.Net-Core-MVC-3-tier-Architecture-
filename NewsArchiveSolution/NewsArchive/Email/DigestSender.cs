using System.Collections.Concurrent;
using System.Threading.Channels;
using BLL.Email;
using BLL.Interfaces;

namespace NewsArchive.Email
{
    public sealed record DigestJob(string Language, int Stories, IReadOnlyList<(int SubscriberId, EmailMessage Message)> Mails);

    public sealed partial class DigestSender : BackgroundService
    {
        private readonly Channel<DigestJob> queue = Channel.CreateUnbounded<DigestJob>();
        private readonly ConcurrentDictionary<string, byte> pending = new();
        private readonly IEmailSender emailSender;
        private readonly IServiceScopeFactory scopeFactory;
        private readonly ILogger<DigestSender> logger;

        public DigestSender(IEmailSender emailSender, IServiceScopeFactory scopeFactory, ILogger<DigestSender> logger)
        {
            this.emailSender = emailSender;
            this.scopeFactory = scopeFactory;
            this.logger = logger;
        }

        public bool CanSend => emailSender.IsEnabled;

        public bool Enqueue(DigestJob job)
        {
            if (!pending.TryAdd(job.Language, 0)) return false;
            queue.Writer.TryWrite(job);
            return true;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var job in queue.Reader.ReadAllAsync(stoppingToken))
            {
                var sent = new List<int>();
                try
                {
                    foreach (var (subscriberId, message) in job.Mails)
                    {
                        if (stoppingToken.IsCancellationRequested) break;
                        if (await emailSender.SendAsync(message, stoppingToken))
                            sent.Add(subscriberId);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    LogFailed(ex, job.Language);
                }
                finally
                {
                    using var scope = scopeFactory.CreateScope();
                    scope.ServiceProvider.GetRequiredService<INewsletterService>().MarkSent(sent, job.Language, job.Stories);
                    pending.TryRemove(job.Language, out _);
                }
                if (sent.Count < job.Mails.Count)
                    LogPartial(job.Language, sent.Count, job.Mails.Count);
            }
        }

        [LoggerMessage(Level = LogLevel.Error, Message = "Sending the {Language} digest failed")]
        private partial void LogFailed(Exception exception, string language);

        [LoggerMessage(Level = LogLevel.Warning, Message = "The {Language} digest reached {Sent} of {Total} subscribers")]
        private partial void LogPartial(string language, int sent, int total);
    }
}
