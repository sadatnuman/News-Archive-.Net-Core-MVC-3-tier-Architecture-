using BLL;
using BLL.Audit;
using BLL.DTOs;
using BLL.Security;

namespace NewsArchive.Tests.Rules
{
    public class ArticleRuleTests
    {
        [Fact]
        public void Admin_can_edit_any_article()
        {
            Assert.True(ArticleRules.CanEdit(authorId: 5, ArticleStatus.Published, userId: 1, AppRoles.Admin));
        }

        [Fact]
        public void Author_can_edit_own_unpublished_article_only()
        {
            Assert.True(ArticleRules.CanEdit(3, ArticleStatus.Draft, 3, AppRoles.Author));
            Assert.True(ArticleRules.CanEdit(3, ArticleStatus.Rejected, 3, AppRoles.Author));
            Assert.False(ArticleRules.CanEdit(3, ArticleStatus.Published, 3, AppRoles.Author));
            Assert.False(ArticleRules.CanEdit(3, ArticleStatus.Draft, 2, AppRoles.Author));
        }

        [Fact]
        public void Reader_cannot_edit()
        {
            Assert.False(ArticleRules.CanEdit(4, ArticleStatus.Draft, 4, AppRoles.Reader));
        }

        [Fact]
        public void Unpublished_article_is_visible_only_to_its_author_and_admins()
        {
            var draft = new ArticleDTO { AuthorId = 3, StatusId = ArticleStatus.Pending, Title = "t", Summary = "s", Content = "c" };
            Assert.True(ArticleRules.CanView(draft, 3, AppRoles.Author));
            Assert.True(ArticleRules.CanView(draft, 1, AppRoles.Admin));
            Assert.False(ArticleRules.CanView(draft, 2, AppRoles.Author));
            Assert.False(ArticleRules.CanView(draft, null, null));
        }

        [Fact]
        public void Schedule_must_be_in_the_future_and_within_a_year()
        {
            var now = new DateTime(2026, 10, 26, 12, 0, 0);
            Assert.True(ArticleRules.IsFutureSchedule(now.AddHours(2), now));
            Assert.False(ArticleRules.IsFutureSchedule(now.AddSeconds(30), now));
            Assert.False(ArticleRules.IsFutureSchedule(now.AddDays(400), now));
            Assert.False(ArticleRules.IsFutureSchedule(null, now));
        }

        [Theory]
        [InlineData(2024, 2, 29, true)]
        [InlineData(2025, 2, 29, false)]
        [InlineData(1989, null, null, false)]
        [InlineData(2026, 13, null, false)]
        [InlineData(2026, null, 5, false)]
        public void Archive_dates_are_validated(int year, int? month, int? day, bool valid)
        {
            Assert.Equal(valid, ArticleRules.IsValidArchiveDate(year, month, day));
        }
    }

    public class SlugTests
    {
        [Theory]
        [InlineData("Dhaka commuters welcome longer hours!", "dhaka-commuters-welcome-longer-hours")]
        [InlineData("   ", "article")]
        [InlineData("Café déjà vu", "cafe-deja-vu")]
        public void Slugs_are_lower_case_and_dashed(string input, string expected)
        {
            Assert.Equal(expected, SlugHelper.Generate(input));
        }

        [Fact]
        public void Bangla_letters_are_kept()
        {
            var slug = SlugHelper.Generate("ঢাকায় সন্ধ্যার রেলসেবা");
            Assert.Equal("ঢাকায়-সন্ধ্যার-রেলসেবা", slug);
        }

        [Fact]
        public void Slugs_are_limited_in_length()
        {
            Assert.True(SlugHelper.Generate(new string('a', 300)).Length <= 80);
        }
    }

    public class PasswordTests
    {
        [Fact]
        public void Hash_verifies_and_never_contains_the_password()
        {
            var hash = PasswordHasher.Hash("Meridian@2026");
            Assert.StartsWith("PBKDF2$100000$", hash);
            Assert.DoesNotContain("Meridian", hash);
            Assert.True(PasswordHasher.Verify("Meridian@2026", hash, out var upgrade));
            Assert.False(upgrade);
            Assert.False(PasswordHasher.Verify("meridian@2026", hash, out _));
        }

        [Fact]
        public void Same_password_gets_a_different_salt()
        {
            Assert.NotEqual(PasswordHasher.Hash("Secret123"), PasswordHasher.Hash("Secret123"));
        }

        [Fact]
        public void Legacy_md5_hash_is_accepted_once_and_flagged_for_upgrade()
        {
            var md5 = Convert.ToHexString(System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes("Meridian@2026"))).ToLowerInvariant();
            Assert.True(PasswordHasher.Verify("Meridian@2026", md5, out var upgrade));
            Assert.True(upgrade);
        }

        [Fact]
        public void Broken_hashes_are_rejected()
        {
            Assert.False(PasswordHasher.Verify("x", "PBKDF2$100000$not-base64$zzz", out _));
            Assert.False(PasswordHasher.Verify("x", "", out _));
        }

        [Fact]
        public void Tokens_are_random_and_hash_to_64_hex_characters()
        {
            var a = PasswordHasher.NewToken();
            var b = PasswordHasher.NewToken();
            Assert.NotEqual(a, b);
            Assert.Equal(64, PasswordHasher.HashToken(a).Length);
            Assert.Equal(64, PasswordHasher.NewHexToken().Length);
        }

        [Theory]
        [InlineData("short1", false)]
        [InlineData("onlyletters", false)]
        [InlineData("12345678", false)]
        [InlineData("Meridian2026", true)]
        public void Strong_password_rule(string password, bool ok)
        {
            Assert.Equal(ok, System.Text.RegularExpressions.Regex.IsMatch(password, PasswordHasher.StrongPasswordPattern));
        }
    }

    public class RecommendationRuleTests
    {
        [Fact]
        public void Keywords_drop_stop_words_short_words_and_numbers()
        {
            var words = RecommendationRules.Keywords("The rail services and the 2026 plan", "More trains for workers");
            Assert.Contains("rail", words);
            Assert.Contains("service", words);
            Assert.Contains("train", words);
            Assert.DoesNotContain("the", words);
            Assert.DoesNotContain("and", words);
            Assert.DoesNotContain("2026", words);
        }

        [Fact]
        public void Bangla_keywords_work_and_stop_words_are_removed()
        {
            var words = RecommendationRules.Keywords("ঢাকায় সন্ধ্যার রেলসেবা এবং যাত্রীরা", null);
            Assert.Contains("রেলসেবা", words);
            Assert.DoesNotContain("এবং", words);
        }

        [Fact]
        public void Similarity_is_zero_to_one()
        {
            var a = new HashSet<string> { "rail", "dhaka", "evening" };
            var b = new HashSet<string> { "rail", "dhaka", "port" };
            Assert.Equal(0.5, RecommendationRules.Similarity(a, b), 3);
            Assert.Equal(0, RecommendationRules.Similarity(a, new HashSet<string>()));
            Assert.Equal(1, RecommendationRules.Similarity(a, a), 3);
        }

        [Fact]
        public void Newer_stories_score_higher_for_freshness()
        {
            var now = DateTime.Now;
            Assert.True(RecommendationRules.Recency(now.AddDays(-1), now) > RecommendationRules.Recency(now.AddDays(-20), now));
            Assert.Equal(1, RecommendationRules.Recency(now.AddDays(1), now), 3);
        }

        [Fact]
        public void Reason_prefers_topics_then_section_then_similar_text_then_author()
        {
            Assert.Equal(RecommendationReason.SharedTopic, RecommendationRules.PickReason(3, 3, 2, 1));
            Assert.Equal(RecommendationReason.SameSection, RecommendationRules.PickReason(0, 1, 2, 1));
            Assert.Equal(RecommendationReason.SimilarStory, RecommendationRules.PickReason(0, 2.5, 2, 1));
            Assert.Equal(RecommendationReason.SameAuthor, RecommendationRules.PickReason(0, 0, 0, 1));
            Assert.Equal(RecommendationReason.Popular, RecommendationRules.PickReason(0, 0, 0, 0));
        }

        [Fact]
        public void Recent_list_puts_the_new_story_first_without_duplicates()
        {
            var merged = RecommendationRules.MergeRecent(new[] { 4, 7, 9, 7, -1 }, 9, 3);
            Assert.Equal(new[] { 9, 4, 7 }, merged);
        }
    }

    public class ModerationAndSearchRuleTests
    {
        [Fact]
        public void Only_known_reasons_are_accepted()
        {
            Assert.True(ModerationRules.IsValidReason(1));
            Assert.True(ModerationRules.IsValidReason(5));
            Assert.False(ModerationRules.IsValidReason(0));
            Assert.False(ModerationRules.IsValidReason(99));
        }

        [Fact]
        public void Notes_are_trimmed_and_limited()
        {
            Assert.Null(ModerationRules.CleanNote("   "));
            Assert.Equal("hi", ModerationRules.CleanNote("  hi "));
            Assert.Equal(ModerationRules.MaxNoteLength, ModerationRules.CleanNote(new string('x', 400))!.Length);
        }

        [Fact]
        public void Comment_is_hidden_on_the_third_report()
        {
            Assert.False(ModerationRules.ShouldAutoHide(2));
            Assert.True(ModerationRules.ShouldAutoHide(3));
        }

        [Theory]
        [InlineData(null, null)]
        [InlineData(" a ", null)]
        [InlineData("  rail   service ", "rail service")]
        public void Search_queries_are_normalized(string? input, string? expected)
        {
            Assert.Equal(expected, SearchRules.NormalizeQuery(input));
        }

        [Fact]
        public void Long_search_queries_are_cut()
        {
            Assert.Equal(SearchRules.MaxQueryLength, SearchRules.NormalizeQuery(new string('q', 200))!.Length);
        }

        [Theory]
        [InlineData("Reader@Example.com", "reader@example.com")]
        [InlineData("not-an-email", null)]
        [InlineData("a@b", null)]
        [InlineData("two words@example.com", null)]
        [InlineData("Name <x@example.com>", null)]
        [InlineData("", null)]
        public void Newsletter_emails_are_validated(string input, string? expected)
        {
            Assert.Equal(expected, NewsletterRules.NormalizeEmail(input));
        }

        [Fact]
        public void Audit_masks_emails_and_clips_details()
        {
            Assert.Equal("n*****@example.com", AuditRules.MaskEmail("Nobody@Example.com"));
            Assert.Equal("(empty)", AuditRules.MaskEmail(" "));
            Assert.Equal(AuditRules.MaxDetailsLength, AuditRules.Clip(new string('d', 900))!.Length);
            Assert.Null(AuditRules.Clip("  "));
        }
    }
}
