using DAL.EF.Tables;

namespace BLL.Builders
{
    public sealed class ArticleBuilder
    {
        private readonly DateTime now;
        private int authorId;
        private string? title;
        private string? summary;
        private string? content;
        private int categoryId;
        private string language = AppLanguages.English;
        private string? imagePath;
        private bool breaking;
        private bool submitted;
        private int? reviewerId;
        private DateTime? publishAt;

        public ArticleBuilder(DateTime now)
        {
            this.now = now;
        }

        public ArticleBuilder WrittenBy(int authorId)
        {
            this.authorId = authorId;
            return this;
        }

        public ArticleBuilder WithText(string? title, string? summary, string? content)
        {
            this.title = title;
            this.summary = summary;
            this.content = content;
            return this;
        }

        public ArticleBuilder InSection(int categoryId)
        {
            this.categoryId = categoryId;
            return this;
        }

        public ArticleBuilder InLanguage(string? language)
        {
            this.language = AppLanguages.Normalize(language);
            return this;
        }

        public ArticleBuilder WithImage(string? imagePath)
        {
            this.imagePath = string.IsNullOrWhiteSpace(imagePath) ? null : imagePath.Trim();
            return this;
        }

        public ArticleBuilder AsBreakingNews(bool breaking = true)
        {
            this.breaking = breaking;
            return this;
        }

        public ArticleBuilder SubmittedForReview()
        {
            submitted = true;
            return this;
        }

        public ArticleBuilder ApprovedBy(int reviewerId, DateTime? publishAt = null)
        {
            this.reviewerId = reviewerId;
            this.publishAt = publishAt;
            return this;
        }

        public Article Build()
        {
            if (authorId <= 0) throw new InvalidOperationException("An article needs an author.");
            if (categoryId <= 0) throw new InvalidOperationException("An article needs a section.");
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(summary) || string.IsNullOrWhiteSpace(content))
                throw new InvalidOperationException("An article needs a title, a summary and text.");
            if (breaking && reviewerId == null)
                throw new InvalidOperationException("Only an approved article can be breaking news.");

            var article = new Article
            {
                AuthorId = authorId,
                Title = title.Trim(),
                Slug = SlugHelper.Generate(title),
                Summary = summary.Trim(),
                Content = content,
                CategoryId = categoryId,
                LanguageCode = language,
                ImagePath = imagePath,
                IsBreakingNews = breaking,
                CreatedAt = now,
                PublishDate = now,
                ViewsCount = 0,
                StatusId = submitted ? ArticleStatus.Pending : ArticleStatus.Draft
            };

            if (reviewerId != null)
            {
                article.ReviewedById = reviewerId;
                article.ReviewedAt = now;
                if (ArticleRules.IsFutureSchedule(publishAt, now))
                {
                    article.StatusId = ArticleStatus.Scheduled;
                    article.ScheduledAt = publishAt;
                    article.PublishDate = publishAt!.Value;
                }
                else
                {
                    article.StatusId = ArticleStatus.Published;
                }
            }
            return article;
        }
    }
}
