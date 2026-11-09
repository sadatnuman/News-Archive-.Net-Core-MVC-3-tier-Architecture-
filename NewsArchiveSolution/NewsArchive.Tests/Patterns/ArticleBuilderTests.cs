using BLL;
using BLL.Builders;
using DAL.EF;

namespace NewsArchive.Tests.Patterns
{
    public class ArticleBuilderTests
    {
        private static readonly DateTime Now = new(2026, 11, 9, 10, 0, 0);

        private static ArticleBuilder Valid() => new ArticleBuilder(Now)
            .WrittenBy(3)
            .WithText("  River ferries return  ", " Ferries run again. ", "Full story text here.")
            .InSection(2);

        [Fact]
        public void An_author_draft_is_not_reviewed_and_gets_a_slug()
        {
            var article = Valid().InLanguage("BN").WithImage("  ").Build();

            Assert.Equal(ArticleStatusIds.Draft, article.StatusId);
            Assert.Null(article.ReviewedById);
            Assert.Equal("River ferries return", article.Title);
            Assert.Equal(SlugHelper.Generate("River ferries return"), article.Slug);
            Assert.Equal(AppLanguages.Bangla, article.LanguageCode);
            Assert.Null(article.ImagePath);
            Assert.False(article.IsBreakingNews);
        }

        [Fact]
        public void A_submitted_article_waits_for_review()
        {
            Assert.Equal(ArticleStatusIds.Pending, Valid().SubmittedForReview().Build().StatusId);
        }

        [Fact]
        public void An_approved_article_without_a_time_is_published_now()
        {
            var article = Valid().ApprovedBy(1).Build();

            Assert.Equal(ArticleStatusIds.Published, article.StatusId);
            Assert.Equal(1, article.ReviewedById);
            Assert.Equal(Now, article.ReviewedAt);
            Assert.Equal(Now, article.PublishDate);
            Assert.Null(article.ScheduledAt);
        }

        [Fact]
        public void An_approved_article_with_a_future_time_is_scheduled()
        {
            var at = Now.AddDays(2);
            var article = Valid().ApprovedBy(1, at).Build();

            Assert.Equal(ArticleStatusIds.Scheduled, article.StatusId);
            Assert.Equal(at, article.ScheduledAt);
            Assert.Equal(at, article.PublishDate);
        }

        [Fact]
        public void A_past_publish_time_publishes_now()
        {
            var article = Valid().ApprovedBy(1, Now.AddHours(-3)).Build();

            Assert.Equal(ArticleStatusIds.Published, article.StatusId);
            Assert.Equal(Now, article.PublishDate);
        }

        [Fact]
        public void Breaking_news_needs_an_approval()
        {
            Assert.Throws<InvalidOperationException>(() => Valid().AsBreakingNews().Build());
            Assert.True(Valid().AsBreakingNews().ApprovedBy(1).Build().IsBreakingNews);
        }

        [Fact]
        public void Missing_author_section_or_text_is_refused()
        {
            Assert.Throws<InvalidOperationException>(() => new ArticleBuilder(Now).WithText("Title here", "Summary", "Body").InSection(2).Build());
            Assert.Throws<InvalidOperationException>(() => new ArticleBuilder(Now).WrittenBy(3).WithText("Title here", "Summary", "Body").Build());
            Assert.Throws<InvalidOperationException>(() => Valid().WithText(" ", "Summary", "Body").Build());
        }
    }
}
