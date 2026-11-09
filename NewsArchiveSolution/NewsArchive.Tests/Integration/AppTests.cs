using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BLL.Email;
using DAL.EF;
using DAL.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NewsArchive.Auth;
using NewsArchive.Email;

namespace NewsArchive.Tests.Integration
{
    [Collection("app")]
    public class AppTests
    {
        private const string Password = "Test@Pass123";
        private readonly AppFactory factory;

        public AppTests(AppFactory factory)
        {
            this.factory = factory;
        }

        private HttpClient Client(bool followRedirects = true)
        {
            var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = followRedirects });
            client.DefaultRequestHeaders.Add(AppFactory.IpHeader, $"10.{Random.Shared.Next(256)}.{Random.Shared.Next(256)}.{Random.Shared.Next(1, 255)}");
            return client;
        }

        private static string NewEmail() => $"t{Guid.NewGuid():N}@test.local";

        private async Task Register(HttpClient client, string email)
        {
            var response = await client.PostAsJsonAsync("/api/auth/register",
                new { name = "Test User", email, password = Password, confirmPassword = Password });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        private static async Task<HttpResponseMessage> Login(HttpClient client, string email, string password) =>
            await client.PostAsJsonAsync("/api/auth/login", new { email, password });

        private async Task<HttpClient> SignedIn(int? roleId = null)
        {
            var client = Client();
            var email = NewEmail();
            await Register(client, email);
            if (roleId.HasValue)
                AppFactory.Execute("UPDATE Users SET RoleId = @role WHERE Email = @email", ("@role", roleId.Value), ("@email", email));
            var response = await Login(client, email, Password);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json.RootElement.GetProperty("token").GetString());
            return client;
        }

        private static async Task<int> Id(HttpResponseMessage response, string property)
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty(property).GetInt32();
        }

        private static async Task<int> CreateSection(HttpClient admin) =>
            await Id(await admin.PostAsJsonAsync("/api/categories", new { name = "Section " + Guid.NewGuid().ToString("N")[..8] }), "categoryId");

        private static async Task<int> WriteStory(HttpClient writer, int section, string content, bool submit = false) =>
            await Id(await writer.PostAsJsonAsync("/api/articles" + (submit ? "?submit=true" : ""), new
            {
                title = "River ferry service returns",
                summary = "Ferries are running again on the southern river routes.",
                content,
                categoryId = section,
                languageCode = "en"
            }), "articleId");

        private static async Task<int> PublishStory(HttpClient admin, string content) =>
            await WriteStory(admin, await CreateSection(admin), content);

        [Fact]
        public async Task Admin_pages_need_sign_in()
        {
            var response = await Client(followRedirects: false).GetAsync("/Admin");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Auth/Login", response.Headers.Location!.OriginalString);
        }

        [Fact]
        public async Task Forms_without_an_antiforgery_token_are_rejected()
        {
            var response = await Client().PostAsync("/Auth/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = "admin@test.local",
                ["Password"] = Password
            }));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Pages_send_security_headers()
        {
            var response = await Client().GetAsync("/Auth/Login");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("script-src 'self'", response.Headers.GetValues("Content-Security-Policy").Single());
            Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
            Assert.Equal("SAMEORIGIN", response.Headers.GetValues("X-Frame-Options").Single());
        }

        [Fact]
        public async Task Development_sign_in_page_offers_demo_accounts()
        {
            var html = await Client().GetStringAsync("/Auth/Login");

            Assert.Contains("data-demo-password", html);
        }

        [Fact]
        public async Task Api_checks_roles()
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await Client().GetAsync("/api/articles/pending")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await (await SignedIn()).GetAsync("/api/articles/pending")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await (await SignedIn(roleId: 1)).GetAsync("/api/articles/pending")).StatusCode);
        }

        [Fact]
        public async Task Five_wrong_passwords_lock_the_account()
        {
            var client = Client();
            var email = NewEmail();
            await Register(client, email);
            for (var i = 0; i < 5; i++)
                Assert.Equal(HttpStatusCode.Unauthorized, (await Login(client, email, "Wrong@Pass1")).StatusCode);

            Assert.Equal(HttpStatusCode.TooManyRequests, (await Login(client, email, Password)).StatusCode);
        }

        [Fact]
        public async Task Search_finds_published_stories_by_word_start()
        {
            var admin = await SignedIn(roleId: 1);
            var token = "zq" + Guid.NewGuid().ToString("N")[..10];
            var id = await PublishStory(admin, $"Boats leave the {token} jetty every hour. নদীবন্দর থেকে প্রতি ঘণ্টায় নৌকা ছাড়ে।");

            async Task<string> Search(string q) =>
                await Client().GetStringAsync("/api/articles?lang=en&q=" + Uri.EscapeDataString(q));

            Assert.Contains($"\"articleId\":{id}", await Search(token[..8]));
            Assert.Contains($"\"articleId\":{id}", await Search("নদী " + token));
            Assert.DoesNotContain($"\"articleId\":{id}", await Search(token + "x"));
        }

        [Fact]
        public async Task Story_pages_never_render_scripts_from_markdown()
        {
            var admin = await SignedIn(roleId: 1);
            var id = await PublishStory(admin, "<script>alert(1)</script>\n\n[bad](javascript:alert(1)) and [odd](//evil.test/x) links.");

            var html = await Client().GetStringAsync($"/news/{id}");

            Assert.DoesNotContain("<script>alert", html);
            Assert.DoesNotContain("javascript:alert", html);
            Assert.DoesNotContain("href=\"//evil.test", html);
        }

        [Fact]
        public async Task Two_reviewers_cannot_both_decide_the_same_article()
        {
            var admin = await SignedIn(roleId: 1);
            var author = await SignedIn(roleId: 3);
            var id = await WriteStory(author, await CreateSection(admin), "A story waiting for an editor's decision.", submit: true);

            using var first = factory.Services.CreateScope();
            using var second = factory.Services.CreateScope();
            var firstRepo = first.ServiceProvider.GetRequiredService<IDataAccessFactory>().GetArticleRepository();
            var secondRepo = second.ServiceProvider.GetRequiredService<IDataAccessFactory>().GetArticleRepository();
            var approved = firstRepo.Get(id)!;
            var rejected = secondRepo.Get(id)!;
            Assert.Equal(ArticleStatusIds.Pending, rejected.StatusId);

            approved.StatusId = ArticleStatusIds.Published;
            rejected.StatusId = ArticleStatusIds.Rejected;
            Assert.True(firstRepo.Update(approved));
            Assert.False(secondRepo.Update(rejected));

            using var check = factory.Services.CreateScope();
            Assert.Equal(ArticleStatusIds.Published, check.ServiceProvider.GetRequiredService<IDataAccessFactory>().GetArticleRepository().Get(id)!.StatusId);
        }

        [Fact]
        public async Task Author_submission_stays_hidden_until_an_admin_approves_it()
        {
            var admin = await SignedIn(roleId: 1);
            var author = await SignedIn(roleId: 3);
            var id = await WriteStory(author, await CreateSection(admin), "Draft text that readers must not see before approval.", submit: true);

            Assert.Equal(HttpStatusCode.NotFound, (await Client().GetAsync($"/api/articles/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await author.PostAsJsonAsync($"/api/articles/{id}/approve", new { })).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await admin.PostAsJsonAsync($"/api/articles/{id}/approve", new { })).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await Client().GetAsync($"/api/articles/{id}")).StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, (await admin.PostAsJsonAsync($"/api/articles/{id}/reject", new { note = "late" })).StatusCode);
        }

        [Fact]
        public void Lockout_and_reset_stores_are_one_instance_for_the_whole_app()
        {
            using var first = factory.Services.CreateScope();
            using var second = factory.Services.CreateScope();

            Assert.Same(first.ServiceProvider.GetRequiredService<LoginThrottle>(), second.ServiceProvider.GetRequiredService<LoginThrottle>());
            Assert.Same(first.ServiceProvider.GetRequiredService<PasswordResetStore>(), second.ServiceProvider.GetRequiredService<PasswordResetStore>());
            Assert.IsType<OutboxEmailSender>(first.ServiceProvider.GetRequiredService<IEmailSender>());
        }

        [Fact]
        public async Task Repositories_from_one_factory_share_one_unit_of_work()
        {
            var admin = await SignedIn(roleId: 1);
            var id = await PublishStory(admin, "A story used to check the repository family.");

            using var scope = factory.Services.CreateScope();
            using var other = factory.Services.CreateScope();
            var data = scope.ServiceProvider.GetRequiredService<IDataAccessFactory>();
            var article = data.GetArticleRepository().Get(id)!;
            var section = data.GetCategoryRepository().Get(article.CategoryId)!;

            Assert.Same(section, article.Category);
            Assert.NotSame(article, other.ServiceProvider.GetRequiredService<IDataAccessFactory>().GetArticleRepository().Get(id));
        }
    }
}
