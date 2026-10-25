namespace BLL.DTOs
{
    public class ArticleDTO
    {
        public int ArticleId { get; set; }

        public string Title { get; set; } = null!;

        public string? Slug { get; set; }

        public string Summary { get; set; } = null!;

        public string Content { get; set; } = null!;

        public DateTime PublishDate { get; set; }

        public int ViewsCount { get; set; }

        public bool IsBreakingNews { get; set; }

        public string? ImagePath { get; set; }

        public int CategoryId { get; set; }

        public int AuthorId { get; set; }

        public int StatusId { get; set; }

        public string LanguageCode { get; set; } = AppLanguages.English;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? ReviewedById { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public string? ReviewNote { get; set; }

        public string StatusName => ArticleStatus.Name(StatusId);

        public bool IsPublished => StatusId == ArticleStatus.Published;
    }
}
