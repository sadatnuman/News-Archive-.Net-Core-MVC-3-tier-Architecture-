using BLL.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NewsArchive.Email;

namespace NewsArchive.Tests.Patterns
{
    public class EmailSenderTests
    {
        private static readonly EmailMessage Valid = new("reader@example.com", "Weekly digest", "Text", Category: "digest", UnsubscribeUrl: "https://site.test/u");
        private static readonly EmailMessage Invalid = new("not-an-address", "Subject", "Text");

        private sealed class RecordingTransport : IMailTransport
        {
            public List<EmailMessage> Delivered { get; } = new();
            public Exception? Failure { get; init; }
            public bool Disposed { get; private set; }

            public Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken)
            {
                if (Failure != null) throw Failure;
                Delivered.Add(message);
                return Task.CompletedTask;
            }

            public void Dispose() => Disposed = true;
        }

        private sealed class RecordingSender : EmailSender
        {
            private readonly Exception? failure;
            public List<RecordingTransport> Created { get; } = new();

            public RecordingSender(Exception? failure = null) : base(NullLogger.Instance)
            {
                this.failure = failure;
            }

            protected override IMailTransport CreateTransport()
            {
                var transport = new RecordingTransport { Failure = failure };
                Created.Add(transport);
                return transport;
            }
        }

        private sealed class TestEnvironment : IWebHostEnvironment
        {
            public string EnvironmentName { get; set; } = "Development";
            public string ApplicationName { get; set; } = "NewsArchive";
            public string ContentRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "meridian-outbox-" + Guid.NewGuid().ToString("N"));
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
            public string WebRootPath { get; set; } = string.Empty;
            public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        }

        private static OutboxEmailSender Outbox(TestEnvironment environment) =>
            new(Options.Create(new EmailOptions { OutboxFolder = "outbox" }), environment, NullLogger<OutboxEmailSender>.Instance);

        [Fact]
        public async Task Each_email_gets_a_fresh_transport_from_the_factory_method()
        {
            var sender = new RecordingSender();

            Assert.True(await sender.SendAsync(Valid));
            Assert.True(await sender.SendAsync(Valid));

            Assert.Equal(2, sender.Created.Count);
            Assert.All(sender.Created, t => Assert.True(t.Disposed));
            Assert.Single(sender.Created[0].Delivered);
        }

        [Fact]
        public async Task An_invalid_address_never_reaches_a_transport()
        {
            var sender = new RecordingSender();

            Assert.False(await sender.SendAsync(Invalid));
            Assert.Empty(sender.Created);
        }

        [Fact]
        public async Task A_failing_transport_returns_false_instead_of_crashing_the_page()
        {
            Assert.False(await new RecordingSender(new IOException("disk full")).SendAsync(Valid));
            Assert.False(await new RecordingSender(new System.Net.Mail.SmtpException("server down")).SendAsync(Valid));
        }

        [Fact]
        public async Task Every_sender_honours_the_same_contract()
        {
            var environment = new TestEnvironment();
            IEmailSender[] senders = { new RecordingSender(), Outbox(environment) };
            try
            {
                foreach (var sender in senders)
                {
                    Assert.True(await sender.SendAsync(Valid));
                    Assert.False(await sender.SendAsync(Invalid));
                }
            }
            finally
            {
                if (Directory.Exists(environment.ContentRootPath)) Directory.Delete(environment.ContentRootPath, true);
            }
        }

        [Fact]
        public async Task Development_outbox_writes_the_message_and_production_writes_nothing()
        {
            var development = new TestEnvironment();
            var production = new TestEnvironment { EnvironmentName = "Production" };
            try
            {
                Assert.True(await Outbox(development).SendAsync(Valid));
                Assert.True(await Outbox(production).SendAsync(Valid));

                var file = Assert.Single(Directory.GetFiles(Path.Combine(development.ContentRootPath, "outbox")));
                var text = await File.ReadAllTextAsync(file);
                Assert.Contains("To: reader@example.com", text);
                Assert.Contains("Subject: Weekly digest", text);
                Assert.Contains("List-Unsubscribe: <https://site.test/u>", text);
                Assert.False(Directory.Exists(production.ContentRootPath));
            }
            finally
            {
                if (Directory.Exists(development.ContentRootPath)) Directory.Delete(development.ContentRootPath, true);
            }
        }
    }
}
