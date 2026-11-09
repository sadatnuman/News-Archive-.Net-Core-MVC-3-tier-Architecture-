using BLL.Email;

namespace NewsArchive.Email
{
    public sealed class DisabledEmailSender : IEmailSender
    {
        public bool IsEnabled => false;

        public Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}
