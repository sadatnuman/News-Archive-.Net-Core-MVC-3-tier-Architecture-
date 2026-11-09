using System.Net;
using System.Text.Json;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using NewsArchive.Auth;
using NewsArchive.Content;
using NewsArchive.Tests.Support;
using static NewsArchive.Auth.PasswordResetStore;

namespace NewsArchive.Tests.Security
{
    public class SecurityTests
    {
        private static MemoryCache Cache() => new(new MemoryCacheOptions());

        private static HttpContextAccessor Http(string ip)
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
            return new HttpContextAccessor { HttpContext = context };
        }

        private static string WrongCode(string code) => code == "111111" ? "222222" : "111111";

        [Fact]
        public void Login_lock_applies_to_one_email_from_one_address()
        {
            var http = Http("10.0.0.1");
            var throttle = new LoginThrottle(Cache(), http);
            for (var i = 0; i < LoginThrottle.MaxFailures; i++)
                throttle.RecordFailure(" Admin@Site.test");

            Assert.True(throttle.IsLocked("admin@site.test"));
            http.HttpContext!.Connection.RemoteIpAddress = IPAddress.Parse("10.0.0.2");
            Assert.False(throttle.IsLocked("admin@site.test"));
        }

        [Fact]
        public void Reset_code_works_once_and_only_when_right()
        {
            var store = new PasswordResetStore(Cache());
            var code = store.Issue(7, "a@b.test")!;

            Assert.Equal(VerifyResult.Invalid, store.Verify("a@b.test", WrongCode(code), out _));
            Assert.Equal(VerifyResult.Valid, store.Verify("A@B.test", code, out var userId));
            Assert.Equal(7, userId);
            Assert.Equal(VerifyResult.NotRequested, store.Verify("a@b.test", code, out _));
        }

        [Fact]
        public void Reset_code_is_burned_after_too_many_wrong_tries_and_requests_are_limited()
        {
            var store = new PasswordResetStore(Cache());
            var code = store.Issue(7, "a@b.test")!;
            for (var i = 1; i < MaxAttempts; i++)
                Assert.Equal(VerifyResult.Invalid, store.Verify("a@b.test", WrongCode(code), out _));

            Assert.Equal(VerifyResult.TooManyAttempts, store.Verify("a@b.test", WrongCode(code), out _));
            Assert.Equal(VerifyResult.NotRequested, store.Verify("a@b.test", code, out _));
            for (var i = 1; i < MaxCodesPerWindow; i++)
                Assert.NotNull(store.Issue(7, "a@b.test"));
            Assert.Null(store.Issue(7, "a@b.test"));
        }

        private static (PasswordChanger Changer, Fake<IAuthService> Auth) Changer()
        {
            var (auth, control) = Fake<IAuthService>.Create();
            control.On("VerifyPassword", a => (string?)a[1] == "Old@Pass1")
                   .On("ChangePassword", _ => true)
                   .On("RevokeAllAsync", _ => Task.FromResult(2));
            return (new PasswordChanger(auth, new LoginThrottle(Cache(), Http("10.0.0.1"))), control);
        }

        private static ChangePasswordDTO Form(string? current) =>
            new() { CurrentPassword = current, PasswordHash = "New@Pass1", ConfPassword = "New@Pass1" };

        [Fact]
        public async Task Password_change_needs_the_current_password()
        {
            var (changer, auth) = Changer();
            var session = new TestSession();

            Assert.Equal(PasswordChanger.Outcome.WrongCurrentPassword, await changer.ChangeAsync(session, 3, "u@b.test", Form("guess")));
            Assert.Equal(0, auth.CountOf("ChangePassword"));
            Assert.Equal(PasswordChanger.Outcome.Changed, await changer.ChangeAsync(session, 3, "u@b.test", Form("Old@Pass1")));
            Assert.Equal(1, auth.CountOf("RevokeAllAsync"));
        }

        [Fact]
        public async Task Emailed_code_allows_one_change_without_the_current_password()
        {
            var (changer, auth) = Changer();
            var session = new TestSession();
            PasswordChanger.MarkResetVerified(session);

            Assert.Equal(PasswordChanger.Outcome.Changed, await changer.ChangeAsync(session, 3, "u@b.test", Form(null)));
            Assert.Equal(0, auth.CountOf("VerifyPassword"));
            Assert.False(PasswordChanger.IsResetVerified(session));
        }

        [Theory]
        [InlineData("//evil.test/x")]
        [InlineData("/\\evil.test")]
        [InlineData("javascript:alert(1)")]
        [InlineData("data:text/html,x")]
        public void Unsafe_links_are_rejected(string url)
        {
            Assert.False(ArticleRenderer.IsSafeLink(url));
        }

        [Fact]
        public void Markdown_cannot_inject_html_or_scripts()
        {
            var html = ArticleRenderer.Render("<script>alert(1)</script>\n\n[x](javascript:alert(1)) [y](https://example.com)").ToString()!;

            Assert.DoesNotContain("<script", html);
            Assert.DoesNotContain("javascript:", html);
            Assert.Contains("rel=\"nofollow noopener noreferrer\"", html);
        }

        [Fact]
        public void Base_settings_never_publish_demo_logins()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "NewsArchive", "appsettings.json")))
                dir = dir.Parent;
            using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(dir!.FullName, "NewsArchive", "appsettings.json")));
            var site = json.RootElement.GetProperty("Site");

            Assert.False(site.GetProperty("ShowDemoNotice").GetBoolean());
            Assert.Equal(0, site.GetProperty("DemoAccounts").GetArrayLength());
        }

        private sealed class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> store = new();
            public bool IsAvailable => true;
            public string Id => "test";
            public IEnumerable<string> Keys => store.Keys;
            public void Clear() => store.Clear();
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void Remove(string key) => store.Remove(key);
            public void Set(string key, byte[] value) => store[key] = value;
            public bool TryGetValue(string key, out byte[] value) => store.TryGetValue(key, out value!);
        }
    }
}
