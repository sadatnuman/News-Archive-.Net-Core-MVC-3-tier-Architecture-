using System.Diagnostics.Tracing;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using BLL;
using BLL.Audit;
using BLL.Email;
using BLL.Interfaces;
using DAL.EF;
using DAL.EF.Tables;
using DAL.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewsArchive.Auth;
using NewsArchive.Email;
using NewsArchive.Publishing;

namespace NewsArchive.Tests.Integration
{
    [Collection("app")]
    public partial class HardeningTests
    {
        private const string Password = "Test@Pass123";
        private readonly AppFactory factory;

        public HardeningTests(AppFactory factory)
        {
            this.factory = factory;
        }

        [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
        private static partial Regex FormToken();

        private sealed record Account(HttpClient Client, int UserId, string Email);

        private static string NewEmail(string prefix = "t") => $"{prefix}{Guid.NewGuid():N}@example.test";

        private static HttpClient Client(WebApplicationFactory<Program> app, string? ip = null)
        {
            var client = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            client.DefaultRequestHeaders.Add(AppFactory.IpHeader, ip ?? $"10.{Random.Shared.Next(256)}.{Random.Shared.Next(256)}.{Random.Shared.Next(1, 255)}");
            return client;
        }

        private static async Task<Account> NewAccount(WebApplicationFactory<Program> app, int roleId, string? email = null)
        {
            email ??= NewEmail();
            var client = Client(app);
            Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/auth/register",
                new { name = "Test User", email, password = Password, confirmPassword = Password })).StatusCode);
            AppFactory.Execute("UPDATE Users SET RoleId = @role WHERE Email = @email", ("@role", roleId), ("@email", email));
            var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password = Password });
            using var json = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", json.RootElement.GetProperty("token").GetString());
            var id = json.RootElement.GetProperty("user").GetProperty("userId").GetInt32();
            return new Account(client, id, email);
        }

        private static async Task<HttpClient> Browser(WebApplicationFactory<Program> app, string email)
        {
            var client = Client(app);
            var page = await client.GetStringAsync("/Auth/Login");
            var response = await client.PostAsync("/Auth/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = email,
                ["Password"] = Password,
                ["__RequestVerificationToken"] = FormToken().Match(page).Groups[1].Value
            }));
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            return client;
        }

        private static async Task<int> Created(HttpResponseMessage response, string property)
        {
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty(property).GetInt32();
        }

        private static async Task<int> Section(HttpClient admin) =>
            await Created(await admin.PostAsJsonAsync("/api/categories", new { name = "Section " + Guid.NewGuid().ToString("N")[..8] }), "categoryId");

        private static async Task<int> Story(HttpClient writer, int section, string? imagePath = null, string title = "Harbour lights return to the river") =>
            await Created(await writer.PostAsJsonAsync("/api/articles", new
            {
                title,
                summary = "Lights along the river harbour are working again.",
                content = "The harbour authority finished repairing the lights along the river walk this week.",
                categoryId = section,
                languageCode = "en",
                imagePath
            }), "articleId");

        private static byte[] Png() => new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }.Concat(new byte[120]).ToArray();

        private static MultipartFormDataContent ImageForm(string field)
        {
            var image = new ByteArrayContent(Png());
            image.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return new MultipartFormDataContent { { image, field, "photo.png" } };
        }

        private static async Task<string> Upload(HttpClient writer)
        {
            var response = await writer.PostAsync("/api/media/images", ImageForm("file"));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("path").GetString()!;
        }

        private sealed class IsolatedUploads : IDisposable
        {
            public IsolatedUploads(AppFactory factory)
            {
                Root = Path.Combine(Path.GetTempPath(), "meridian-webroot-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(Root);
                App = factory.WithWebHostBuilder(b => b.UseWebRoot(Root));
            }

            public string Root { get; }

            public WebApplicationFactory<Program> App { get; }

            public string FileOf(string sitePath) => Path.Combine(Root, sitePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            public string Plant(string sitePath)
            {
                var file = FileOf(sitePath);
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                File.WriteAllBytes(file, Png());
                return file;
            }

            public void Dispose()
            {
                App.Dispose();
                if (Directory.Exists(Root)) Directory.Delete(Root, true);
            }
        }

        [Fact]
        public async Task Deleting_one_story_never_removes_a_picture_another_story_still_uses()
        {
            using var uploads = new IsolatedUploads(factory);
            var admin = await NewAccount(uploads.App, AppRoles.Admin);
            var intruder = await NewAccount(uploads.App, AppRoles.Author);
            var section = await Section(admin.Client);
            var picture = await Upload(admin.Client);
            var original = await Story(admin.Client, section, picture);
            var copy = await Story(intruder.Client, section, picture);

            Assert.Equal(HttpStatusCode.NoContent, (await intruder.Client.DeleteAsync($"/api/articles/{copy}")).StatusCode);
            Assert.True(File.Exists(uploads.FileOf(picture)));
            Assert.Equal(Png(), await File.ReadAllBytesAsync(uploads.FileOf(picture)));

            Assert.Equal(HttpStatusCode.NoContent, (await admin.Client.DeleteAsync($"/api/articles/{original}")).StatusCode);
            Assert.False(File.Exists(uploads.FileOf(picture)));
        }

        [Fact]
        public async Task A_story_pointing_at_someone_elses_avatar_never_deletes_it()
        {
            using var uploads = new IsolatedUploads(factory);
            var reader = await NewAccount(uploads.App, AppRoles.Reader);
            var intruder = await NewAccount(uploads.App, AppRoles.Author);
            var admin = await NewAccount(uploads.App, AppRoles.Admin);
            var avatarResponse = await reader.Client.PutAsync("/api/auth/me/avatar", ImageForm("file"));
            Assert.Equal(HttpStatusCode.OK, avatarResponse.StatusCode);
            using var json = JsonDocument.Parse(await avatarResponse.Content.ReadAsStringAsync());
            var avatar = json.RootElement.GetProperty("avatarPath").GetString()!;

            var story = await Story(intruder.Client, await Section(admin.Client), avatar);
            Assert.Equal(HttpStatusCode.NoContent, (await intruder.Client.DeleteAsync($"/api/articles/{story}")).StatusCode);

            Assert.True(File.Exists(uploads.FileOf(avatar)));
        }

        [Fact]
        public void Only_canonical_unused_upload_paths_can_ever_be_deleted()
        {
            using var uploads = new IsolatedUploads(factory);
            var name = Guid.NewGuid().ToString("N");
            var article = $"/uploads/articles/2026/01/{name}.png";
            var avatar = $"/uploads/avatars/{name}.png";
            var plantedArticle = uploads.Plant(article);
            var plantedAvatar = uploads.Plant(avatar);
            var plantedStatic = uploads.Plant("/images/brand/keep.png");

            using var scope = uploads.App.Services.CreateScope();
            var media = scope.ServiceProvider.GetRequiredService<IMediaService>();
            string?[] unsafePaths =
            {
                null, "", avatar, "/images/brand/keep.png", "https://example.test/x.png", "uploads/articles/2026/01/" + name + ".png",
                "/UPLOADS/articles/2026/01/" + name + ".png", "/uploads/articles/2026/01/" + name.ToUpperInvariant() + ".png",
                "/uploads/articles/2026/01/../../avatars/" + name + ".png", "/uploads//articles/2026/01/" + name + ".png",
                "%2Fuploads%2Farticles%2F2026%2F01%2F" + name + ".png", "/uploads/articles/2026/01/" + name + ".png.exe", "\\uploads\\articles\\2026\\01\\" + name + ".png"
            };
            foreach (var path in unsafePaths)
                Assert.False(media.DeleteArticleImageIfUnused(path), path);
            Assert.False(media.DeleteAvatarIfUnused(article));

            Assert.True(File.Exists(plantedArticle));
            Assert.True(File.Exists(plantedAvatar));
            Assert.True(File.Exists(plantedStatic));

            Assert.True(media.DeleteArticleImageIfUnused(article));
            Assert.False(File.Exists(plantedArticle));
            Assert.True(media.DeleteAvatarIfUnused(avatar));
            Assert.False(File.Exists(plantedAvatar));
        }

        [Fact]
        public async Task A_form_for_a_missing_section_keeps_the_text_and_removes_only_its_own_upload()
        {
            using var uploads = new IsolatedUploads(factory);
            var author = await NewAccount(uploads.App, AppRoles.Author);
            var bystander = uploads.Plant($"/uploads/articles/2026/01/{Guid.NewGuid():N}.png");
            var browser = await Browser(uploads.App, author.Email);
            var page = await browser.GetStringAsync("/Article/Create");
            var title = "Unsaved story " + Guid.NewGuid().ToString("N")[..8];

            var image = new ByteArrayContent(Png());
            image.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            var form = new MultipartFormDataContent
            {
                { new StringContent(title), "Title" },
                { new StringContent("A summary that is long enough to pass."), "Summary" },
                { new StringContent("Body text that is long enough to pass validation."), "Content" },
                { new StringContent("987654321"), "CategoryId" },
                { new StringContent("en"), "LanguageCode" },
                { new StringContent(FormToken().Match(page).Groups[1].Value), "__RequestVerificationToken" },
                { image, "imageFile", "photo.png" }
            };
            var response = await browser.PostAsync("/Article/Create", form);
            var html = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(title, html);
            Assert.Contains("This section no longer exists", html);
            Assert.True(File.Exists(bystander));
            Assert.Single(Directory.GetFiles(Path.Combine(uploads.Root, "uploads", "articles"), "*", SearchOption.AllDirectories));
        }

        [Fact]
        public async Task Saving_into_a_section_that_vanished_returns_false_instead_of_crashing()
        {
            var author = await NewAccount(factory, AppRoles.Author);
            using var scope = factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IDataAccessFactory>().GetArticleRepository();

            Assert.False(repo.Create(new Article
            {
                Title = "Orphan story",
                Summary = "Summary of the orphan story.",
                Content = "Content of the orphan story that is long enough.",
                CategoryId = 987654321,
                AuthorId = author.UserId,
                StatusId = ArticleStatusIds.Draft,
                LanguageCode = "en",
                CreatedAt = SiteClock.Now
            }));
            Assert.False(repo.UsesImage("/uploads/articles/2026/01/none.png"));
        }

        [Fact]
        public async Task Twelve_parallel_wrong_passwords_get_five_checks_and_then_both_sign_in_paths_are_locked()
        {
            var victim = await NewAccount(factory, AppRoles.Reader);
            var ip = $"10.250.{Random.Shared.Next(256)}.{Random.Shared.Next(1, 255)}";
            var attacker = Client(factory, ip);

            var responses = await Task.WhenAll(Enumerable.Range(0, 12).Select(_ =>
                attacker.PostAsJsonAsync("/api/auth/login", new { email = victim.Email, password = "Wrong@Pass1" })));

            Assert.Equal(LoginThrottle.MaxFailures, responses.Count(r => r.StatusCode == HttpStatusCode.Unauthorized));
            Assert.Equal(12 - LoginThrottle.MaxFailures, responses.Count(r => r.StatusCode == HttpStatusCode.TooManyRequests));
            Assert.Equal(HttpStatusCode.TooManyRequests, (await attacker.PostAsJsonAsync("/api/auth/login", new { email = victim.Email, password = Password })).StatusCode);

            var page = await attacker.GetStringAsync("/Auth/Login");
            var mvc = await attacker.PostAsync("/Auth/Login", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Email"] = victim.Email,
                ["Password"] = Password,
                ["__RequestVerificationToken"] = FormToken().Match(page).Groups[1].Value
            }));
            Assert.Equal(HttpStatusCode.OK, mvc.StatusCode);
            Assert.Contains("Too many failed attempts", await mvc.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Two_registrations_for_one_email_at_once_create_exactly_one_account()
        {
            var email = NewEmail("dup");
            var responses = await Task.WhenAll(Enumerable.Range(0, 2).Select(_ => Client(factory).PostAsJsonAsync("/api/auth/register",
                new { name = "Twin", email, password = Password, confirmPassword = Password })));

            Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
            Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
            Assert.Equal(1, (int)AppFactory.Scalar("SELECT COUNT(*) FROM Users WHERE Email = @e", ("@e", email))!);
            Assert.Equal(1, (int)AppFactory.Scalar("SELECT COUNT(*) FROM AuditLogs a JOIN Users u ON a.EntityId = u.UserId WHERE u.Email = @e AND a.Action = @a",
                ("@e", email), ("@a", AuditActions.AccountRegistered))!);
        }

        [Fact]
        public void A_duplicate_insert_returns_false_and_the_repository_keeps_working()
        {
            var email = NewEmail("dup");
            User NewUser() => new() { Name = "Twin", Email = email, PasswordHash = "x", RoleId = AppRoles.Reader, CreatedAt = SiteClock.Now };
            using var first = factory.Services.CreateScope();
            using var second = factory.Services.CreateScope();
            var firstRepo = first.ServiceProvider.GetRequiredService<IDataAccessFactory>().GetUserRepository();
            var secondRepo = second.ServiceProvider.GetRequiredService<IDataAccessFactory>().GetUserRepository();

            Assert.True(firstRepo.Create(NewUser()));
            Assert.False(secondRepo.Create(NewUser()));
            Assert.NotNull(secondRepo.GetByEmail(email));
        }

        private async Task<(Account Moderator, int OwnComment, int ModeratedComment, int ReportedComment)> ModeratorWithHistory(string email)
        {
            var moderator = await NewAccount(factory, AppRoles.Admin, email);
            var reader = await NewAccount(factory, AppRoles.Reader);
            var story = await Story(moderator.Client, await Section(moderator.Client));
            async Task<int> Comment(HttpClient who, string text) =>
                await Created(await who.PostAsJsonAsync($"/api/articles/{story}/comments", new { text }), "commentId");

            var own = await Comment(moderator.Client, "A comment by the moderator.");
            var moderated = await Comment(reader.Client, "A comment the moderator will hide.");
            var reported = await Comment(reader.Client, "A comment the moderator reports.");
            Assert.Equal(HttpStatusCode.NoContent, (await moderator.Client.PostAsync($"/api/comments/{moderated}/hide", null)).StatusCode);
            Assert.True((await moderator.Client.PostAsJsonAsync($"/api/comments/{reported}/report", new { reasonId = 1 })).IsSuccessStatusCode);
            AppFactory.Execute("UPDATE Articles SET AuthorId = (SELECT TOP 1 UserId FROM Users WHERE UserId <> @id ORDER BY UserId), ReviewedById = NULL WHERE AuthorId = @id",
                ("@id", moderator.UserId));
            return (moderator, own, moderated, reported);
        }

        private static int Count(string sql, int id) => (int)AppFactory.Scalar(sql, ("@id", id))!;

        [Fact]
        public async Task Deleting_a_user_removes_their_comments_reports_and_moderator_links_together()
        {
            var (moderator, own, moderated, _) = await ModeratorWithHistory(NewEmail());
            using var scope = factory.Services.CreateScope();

            Assert.True(scope.ServiceProvider.GetRequiredService<IDataAccessFactory>().GetUserRepository().Delete(moderator.UserId));

            Assert.Equal(0, Count("SELECT COUNT(*) FROM Users WHERE UserId = @id", moderator.UserId));
            Assert.Equal(0, Count("SELECT COUNT(*) FROM Comments WHERE CommentId = @id", own));
            Assert.Equal(0, Count("SELECT COUNT(*) FROM CommentReports WHERE UserId = @id", moderator.UserId));
            Assert.Equal(0, Count("SELECT COUNT(*) FROM Comments WHERE CommentId = @id AND ModeratedById IS NOT NULL", moderated));
        }

        [Fact]
        public async Task A_user_delete_that_fails_at_the_last_step_rolls_everything_back()
        {
            var (moderator, own, moderated, _) = await ModeratorWithHistory(NewEmail("rollback-probe-"));
            var trigger = "trg_probe_" + Guid.NewGuid().ToString("N")[..8];
            AppFactory.Execute($"CREATE TRIGGER {trigger} ON Users AFTER DELETE AS IF EXISTS (SELECT 1 FROM deleted WHERE Email LIKE 'rollback-probe-%') THROW 50001, 'Injected failure', 1;");
            try
            {
                using var scope = factory.Services.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IDataAccessFactory>().GetUserRepository();
                Assert.ThrowsAny<Exception>(() => repo.Delete(moderator.UserId));
            }
            finally
            {
                AppFactory.Execute($"DROP TRIGGER {trigger};");
            }

            Assert.Equal(1, Count("SELECT COUNT(*) FROM Users WHERE UserId = @id", moderator.UserId));
            Assert.Equal(1, Count("SELECT COUNT(*) FROM Comments WHERE CommentId = @id", own));
            Assert.Equal(1, Count("SELECT COUNT(*) FROM CommentReports WHERE UserId = @id", moderator.UserId));
            Assert.Equal(moderator.UserId, (int)AppFactory.Scalar("SELECT ModeratedById FROM Comments WHERE CommentId = @id", ("@id", moderated))!);
        }

        [Fact]
        public async Task The_edit_api_explains_a_missing_version_and_keeps_409_for_real_conflicts()
        {
            var admin = await NewAccount(factory, AppRoles.Admin);
            var otherAuthor = await NewAccount(factory, AppRoles.Author);
            var id = await Story(admin.Client, await Section(admin.Client));
            using var loaded = JsonDocument.Parse(await Client(factory).GetStringAsync($"/api/articles/{id}"));
            var story = loaded.RootElement;
            object Body(string? version) => new
            {
                title = "Harbour lights shine again",
                summary = story.GetProperty("summary").GetString(),
                content = story.GetProperty("content").GetString(),
                categoryId = story.GetProperty("categoryId").GetInt32(),
                languageCode = "en",
                version
            };

            var missing = await admin.Client.PutAsJsonAsync($"/api/articles/{id}", Body(null));
            Assert.Equal(HttpStatusCode.BadRequest, missing.StatusCode);
            var problem = await missing.Content.ReadAsStringAsync();
            Assert.Contains("\"version\"", problem);
            Assert.Contains("GET /api/articles/{id}", problem);
            Assert.DoesNotContain("Someone else changed", problem);
            Assert.Equal(HttpStatusCode.BadRequest, (await admin.Client.PutAsJsonAsync($"/api/articles/{id}", Body("not-a-version"))).StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, (await admin.Client.PutAsJsonAsync($"/api/articles/{id}", Body("3-1"))).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await admin.Client.PutAsJsonAsync("/api/articles/987654321", Body("3-1"))).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await otherAuthor.Client.PutAsJsonAsync($"/api/articles/{id}", Body(story.GetProperty("version").GetString()))).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await Client(factory).PutAsJsonAsync($"/api/articles/{id}", Body(story.GetProperty("version").GetString()))).StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, (await admin.Client.PutAsJsonAsync($"/api/articles/{id}", Body(story.GetProperty("version").GetString()))).StatusCode);

            var swagger = await Client(factory).GetStringAsync("/swagger/v1/swagger.json");
            Assert.Contains("A missing or malformed version gives 400", swagger);
        }

        [Fact]
        public async Task A_delete_racing_an_edit_fails_cleanly_instead_of_crashing()
        {
            var admin = await NewAccount(factory, AppRoles.Admin);
            var id = await Story(admin.Client, await Section(admin.Client));
            using var deleter = factory.Services.CreateScope();
            deleter.ServiceProvider.GetRequiredService<IDataAccessFactory>().GetArticleRepository().Get(id);

            using (var editor = factory.Services.CreateScope())
            {
                var service = editor.ServiceProvider.GetRequiredService<IArticleService>();
                var form = service.GetForm(id)!;
                form.Title = "Edited while someone deletes";
                Assert.Equal(ChangeResult.Done, service.Update(id, form, admin.UserId, AppRoles.Admin, false));
            }

            Assert.False(deleter.ServiceProvider.GetRequiredService<IArticleService>().Delete(id, admin.UserId, AppRoles.Admin));
            Assert.Equal(1, Count("SELECT COUNT(*) FROM Articles WHERE ArticleId = @id", id));
        }

        [Fact]
        public async Task Two_reports_cannot_hide_a_comment_twice()
        {
            var admin = await NewAccount(factory, AppRoles.Admin);
            var story = await Story(admin.Client, await Section(admin.Client));
            var comment = await Created(await admin.Client.PostAsJsonAsync($"/api/articles/{story}/comments", new { text = "A comment to hide." }), "commentId");
            using var scope = factory.Services.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IDataAccessFactory>().GetCommentRepository();

            Assert.True(repo.HideIfVisible(comment, SiteClock.Now));
            Assert.False(repo.HideIfVisible(comment, SiteClock.Now));
        }

        [Fact]
        public async Task Daily_maintenance_removes_expired_tokens_and_old_audit_entries()
        {
            var user = await NewAccount(factory, AppRoles.Reader);
            var hash = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            var marker = "maintenance-probe-" + Guid.NewGuid().ToString("N");
            AppFactory.Execute("INSERT INTO UserTokens (UserId, TokenHash, ClientType, CreatedAt, ExpiresAt) VALUES (@u, @h, 'api', DATEADD(day, -90, SYSUTCDATETIME()), DATEADD(day, -60, SYSUTCDATETIME()))",
                ("@u", user.UserId), ("@h", hash));
            AppFactory.Execute("INSERT INTO AuditLogs (Action, EntityType, Details, CreatedAt) VALUES ('probe', 'Security', @d, DATEADD(day, -400, GETDATE()))", ("@d", marker));

            factory.Services.GetRequiredService<MaintenanceJob>().RunOnce();

            Assert.Equal(0, (int)AppFactory.Scalar("SELECT COUNT(*) FROM UserTokens WHERE TokenHash = @h", ("@h", hash))!);
            Assert.Equal(0, (int)AppFactory.Scalar("SELECT COUNT(*) FROM AuditLogs WHERE Details = @d", ("@d", marker))!);
            Assert.Equal(TimeSpan.FromHours(24), MaintenanceJob.Interval);
        }

        [Fact]
        public async Task The_news_sitemap_lists_only_recent_stories_and_feeds_are_cached_briefly()
        {
            var admin = await NewAccount(factory, AppRoles.Admin);
            var section = await Section(admin.Client);
            var old = await Story(admin.Client, section, title: "An older harbour story");
            var fresh = await Story(admin.Client, section, title: "A fresh harbour story");
            AppFactory.Execute("UPDATE Articles SET PublishDate = DATEADD(day, -10, PublishDate) WHERE ArticleId = @id", ("@id", old));
            var anonymous = Client(factory);

            var news = await anonymous.GetStringAsync("/news-sitemap.xml");
            Assert.Contains($"/news/{fresh}/", news);
            Assert.DoesNotContain($"/news/{old}/", news);
            Assert.Contains($"/news/{old}/", await anonymous.GetStringAsync("/sitemap.xml"));
            var rss = await anonymous.GetAsync("/rss.xml");
            Assert.Equal(TimeSpan.FromMinutes(5), rss.Headers.CacheControl!.MaxAge);
        }

        private sealed class SocketWatcher : EventListener
        {
            public List<string> Connections { get; } = new();

            protected override void OnEventSourceCreated(EventSource source)
            {
                if (source.Name == "System.Net.Sockets")
                    EnableEvents(source, EventLevel.Informational);
            }

            protected override void OnEventWritten(EventWrittenEventArgs e)
            {
                if (e.EventName == "ConnectStart" && e.Payload is { Count: > 0 })
                    lock (Connections) Connections.Add(e.Payload[0]?.ToString() ?? string.Empty);
            }
        }

        private sealed class LogCapture : ILoggerProvider
        {
            public List<(string Category, LogLevel Level, string Message)> Entries { get; } = new();

            public ILogger CreateLogger(string categoryName) => new Logger(this, categoryName);

            public void Dispose()
            {
            }

            private sealed class Logger : ILogger
            {
                private readonly LogCapture owner;
                private readonly string category;

                public Logger(LogCapture owner, string category)
                {
                    this.owner = owner;
                    this.category = category;
                }

                public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

                public bool IsEnabled(LogLevel logLevel) => true;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                {
                    lock (owner.Entries) owner.Entries.Add((category, logLevel, formatter(state, exception)));
                }
            }
        }

        [Fact]
        public async Task Outside_development_mail_is_disabled_even_with_an_smtp_host_and_sends_nothing()
        {
            var logs = new LogCapture();
            using var app = factory.WithWebHostBuilder(b =>
            {
                b.UseEnvironment("Production");
                b.UseSetting("Email:Smtp:Host", "smtp.example.invalid");
                b.UseSetting("Email:Smtp:Port", "587");
                b.ConfigureLogging(l => l.AddProvider(logs));
            });
            var sender = app.Services.GetRequiredService<IEmailSender>();
            Assert.IsType<DisabledEmailSender>(sender);
            Assert.False(sender.IsEnabled);
            Assert.Single(app.Services.GetServices<IEmailSender>());
            Assert.False(app.Services.GetRequiredService<DigestSender>().CanSend);
            var before = logs.Entries.Count(e => e.Level >= LogLevel.Warning);

            using var sockets = new SocketWatcher();
            var results = await Task.WhenAll(Enumerable.Range(0, 2000).Select(i =>
                sender.SendAsync(new EmailMessage($"reader{i}@example.test", "Digest", "Text", Category: "digest"))));

            Assert.All(results, Assert.False);
            Assert.Equal(before, logs.Entries.Count(e => e.Level >= LogLevel.Warning));
            Assert.Single(logs.Entries, e => e.Message.StartsWith("Email delivery is disabled", StringComparison.Ordinal));
            lock (sockets.Connections)
                Assert.DoesNotContain(sockets.Connections, c => c.EndsWith(":25", StringComparison.Ordinal) || c.EndsWith(":465", StringComparison.Ordinal)
                    || c.EndsWith(":587", StringComparison.Ordinal) || c.EndsWith(":2525", StringComparison.Ordinal) || c.Contains("smtp", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Newsletter_double_opt_in_works_offline_through_the_private_outbox()
        {
            var sender = factory.Services.GetRequiredService<IEmailSender>();
            Assert.IsType<OutboxEmailSender>(sender);
            var environment = factory.Services.GetRequiredService<IWebHostEnvironment>();
            var outbox = Path.Combine(environment.ContentRootPath, "App_Data", "outbox");
            Assert.False(Path.GetFullPath(outbox).StartsWith(Path.GetFullPath(environment.WebRootPath), StringComparison.OrdinalIgnoreCase));

            var email = NewEmail("newsletter");
            var browser = Client(factory);
            async Task<HttpResponseMessage> Subscribe(string lang)
            {
                var page = await browser.GetStringAsync("/?lang=" + lang);
                var request = new HttpRequestMessage(HttpMethod.Post, "/newsletter/subscribe?lang=" + lang)
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["email"] = email,
                        ["__RequestVerificationToken"] = FormToken().Match(page).Groups[1].Value
                    })
                };
                request.Headers.Accept.ParseAdd("application/json");
                return await browser.SendAsync(request);
            }

            string[] files = Array.Empty<string>();
            try
            {
                Assert.Equal(HttpStatusCode.OK, (await Subscribe("en")).StatusCode);
                files = Directory.GetFiles(outbox).Where(f => File.ReadAllText(f).Contains("To: " + email)).ToArray();
                var mail = await File.ReadAllTextAsync(Assert.Single(files));
                var confirm = Regex.Match(mail, @"/newsletter/confirm\?token=([A-Za-z0-9%]+)").Groups[1].Value;
                var unsubscribe = Regex.Match(mail, @"/newsletter/unsubscribe\?token=([A-Za-z0-9%]+)").Groups[1].Value;

                Assert.Equal(HttpStatusCode.OK, (await browser.GetAsync("/newsletter/confirm?token=" + confirm)).StatusCode);
                Assert.NotNull(AppFactory.Scalar("SELECT ConfirmedAt FROM Subscribers WHERE Email = @e", ("@e", email)) as DateTime?);

                Assert.Equal(HttpStatusCode.OK, (await Subscribe("bn")).StatusCode);
                Assert.Equal("en", AppFactory.Scalar("SELECT LanguageCode FROM Subscribers WHERE Email = @e", ("@e", email)));

                var leave = await browser.PostAsync("/newsletter/unsubscribe", new FormUrlEncodedContent(new Dictionary<string, string> { ["token"] = Uri.UnescapeDataString(unsubscribe) }));
                Assert.Equal(HttpStatusCode.OK, leave.StatusCode);
                Assert.NotNull(AppFactory.Scalar("SELECT UnsubscribedAt FROM Subscribers WHERE Email = @e", ("@e", email)) as DateTime?);
            }
            finally
            {
                foreach (var file in files)
                    File.Delete(file);
                AppFactory.Execute("DELETE FROM Subscribers WHERE Email = @e", ("@e", email));
            }
        }
    }
}
