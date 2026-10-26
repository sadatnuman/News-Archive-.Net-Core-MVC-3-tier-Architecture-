using BLL;
using BLL.Security;
using BLL.Services;
using DAL.EF.Tables;
using NewsArchive.Tests.Support;

namespace NewsArchive.Tests.Services
{
    public class NewsletterServiceTests
    {
        private readonly FakeFactory data = new();
        private readonly RecordingAudit audit = new();
        private readonly List<Subscriber> store = new();

        private NewsletterService Service()
        {
            data.Subscribers
                .On("GetByEmail", a => store.FirstOrDefault(s => s.Email == (string)a[0]!))
                .On("GetByConfirmHash", a => store.FirstOrDefault(s => s.ConfirmTokenHash == (string)a[0]!))
                .On("GetByUnsubscribeToken", a => store.FirstOrDefault(s => s.UnsubscribeToken == (string)a[0]!))
                .On("Create", a =>
                {
                    var s = (Subscriber)a[0]!;
                    s.SubscriberId = store.Count + 1;
                    store.Add(s);
                    return true;
                })
                .On("Save", _ => true);
            return new NewsletterService(data.Instance, TestServices.Mapper, audit);
        }

        [Fact]
        public void Invalid_email_is_rejected()
        {
            Assert.Equal(SubscribeStatus.InvalidEmail, Service().Subscribe("nope", "en").Status);
            Assert.Empty(store);
        }

        [Fact]
        public void New_subscriber_needs_confirmation_and_only_the_token_hash_is_stored()
        {
            var result = Service().Subscribe(" Reader@Example.com ", "bn");
            Assert.Equal(SubscribeStatus.ConfirmationNeeded, result.Status);
            var saved = Assert.Single(store);
            Assert.Equal("reader@example.com", saved.Email);
            Assert.Equal("bn", saved.LanguageCode);
            Assert.Null(saved.ConfirmedAt);
            Assert.NotEqual(result.ConfirmToken, saved.ConfirmTokenHash);
            Assert.Equal(PasswordHasher.HashToken(result.ConfirmToken!), saved.ConfirmTokenHash);
        }

        [Fact]
        public void Asking_again_within_two_minutes_sends_nothing()
        {
            var service = Service();
            service.Subscribe("reader@example.com", "en");
            Assert.Equal(SubscribeStatus.TooSoon, service.Subscribe("reader@example.com", "en").Status);
        }

        [Fact]
        public void Confirmation_link_activates_the_subscription_once()
        {
            var service = Service();
            var token = service.Subscribe("reader@example.com", "en").ConfirmToken!;
            Assert.Equal(ConfirmStatus.Confirmed, service.Confirm(token).Status);
            Assert.NotNull(store[0].ConfirmedAt);
            Assert.Equal(ConfirmStatus.AlreadyConfirmed, service.Confirm(token.ToLowerInvariant()).Status);
            Assert.Equal(SubscribeStatus.AlreadySubscribed, service.Subscribe("reader@example.com", "en").Status);
        }

        [Fact]
        public void Old_confirmation_links_expire()
        {
            var service = Service();
            var token = service.Subscribe("reader@example.com", "en").ConfirmToken!;
            store[0].CreatedAt = DateTime.Now.AddDays(-3);
            Assert.Equal(ConfirmStatus.Expired, service.Confirm(token).Status);
        }

        [Fact]
        public void Wrong_or_oversized_tokens_are_not_found()
        {
            var service = Service();
            Assert.Equal(ConfirmStatus.NotFound, service.Confirm("abc").Status);
            Assert.Equal(ConfirmStatus.NotFound, service.Confirm(new string('A', 5000)).Status);
            Assert.False(service.Unsubscribe(null));
        }

        [Fact]
        public void Unsubscribe_marks_the_reader_and_a_new_subscribe_starts_over()
        {
            var service = Service();
            var result = service.Subscribe("reader@example.com", "en");
            service.Confirm(result.ConfirmToken);
            Assert.True(service.Unsubscribe(result.UnsubscribeToken));
            Assert.NotNull(store[0].UnsubscribedAt);
            Assert.Equal(SubscribeStatus.ConfirmationNeeded, service.Subscribe("reader@example.com", "en").Status);
            Assert.Null(store[0].UnsubscribedAt);
            Assert.Null(store[0].ConfirmedAt);
        }
    }
}
