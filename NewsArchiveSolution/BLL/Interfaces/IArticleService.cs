using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IArticleService
    {
        List<ArticleDTO> Get();

        ArticleDTO? Get(int id);

        int Create(ArticleFormDTO form, int authorId, int roleId, bool submitForReview);

        bool Update(int articleId, ArticleFormDTO form, int editorId, int roleId, bool submitForReview);

        bool Submit(int articleId, int authorId);

        bool Approve(int articleId, int adminId, DateTime? publishAt = null);

        bool PublishNow(int articleId);

        bool Unschedule(int articleId);

        int PublishDue();

        List<ArticleDTO> GetScheduled();

        bool Reject(int articleId, int adminId, string? note);

        bool Delete(int articleId, int userId, int roleId);

        ArticleFormDTO? GetForm(int id);

        List<ArticleDTO> GetByAuthor(int authorId);

        List<ArticleDTO> GetPending();

        int CountPending();

        List<ArticleDTO> GetByCategoryId(int categoryId, string? language = null);

        ArticleDTO? GetWithDetails(int id);

        void IncrementViewCount(int articleId);

        List<ArticleDTO> Search(string? keyword, DateTime? from, DateTime? to, int page, int pageSize, out int total, string? language = null);

        List<ArticleDTO> GetArchive(int year, int? month, int? day, string? language, int page, int pageSize, out int total);

        List<ArchiveMonthDTO> GetArchiveMonths(string? language = null);

        Dictionary<int, int> GetArchiveDayCounts(int year, int month, string? language = null);

        List<ArticleDTO> GetOnThisDay(DateTime today, int count, string? language = null);

        List<ArticleDTO> GetBreakingNews(string? language = null);

        List<ArticleDTO> GetTrendingArticles(int count, string? language = null);

        List<ArticleDTO> GetLatest(int count, string? language = null);

        List<ArticleDTO> GetRecommendedArticles(int articleId, int count = 4);

        List<ArticleDTO> GetPublishedForSitemap();

        int GetCommentCount(int articleId);

        bool HasBreakingNews(string? language = null);

        void EnsureSlugs();
    }
}
