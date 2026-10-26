using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface IArticleRepository : IRepository<Article>
    {
        List<Article> GetByCategoryId(int categoryId, string? language = null);

        Article? GetWithDetails(int id);

        void IncrementViewCount(int articleId);

        List<Article> Search(string? keyword, DateTime? from, DateTime? to, int page, int pageSize, out int total, string? language = null);

        List<Article> GetByPeriod(DateTime from, DateTime toExclusive, string? language, int page, int pageSize, out int total);

        List<(int Year, int Month, int Count)> CountByMonth(string? language = null);

        Dictionary<int, int> CountByDay(int year, int month, string? language = null);

        List<Article> GetOnThisDay(int month, int day, int beforeYear, int count, string? language = null);

        List<Article> GetBreaking(string? language = null);

        bool HasBreakingNews(string? language = null);

        List<Article> GetTrending(int count, string? language = null);

        List<Article> GetMostViewed(int count);

        List<Article> GetLatest(int count, string? language = null);

        List<Article> GetRecommended(int articleId, int count);

        List<Article> GetByAuthor(int authorId);

        List<Article> GetPublishedByAuthor(int authorId, string? language, int page, int pageSize, out int total);

        Dictionary<int, int> CountPublishedByAuthor(string? language = null);

        List<Article> GetByStatus(int statusId);

        List<Article> GetPublishedForSitemap();

        List<Article> GetWithoutSlug();

        int CountByStatus(int statusId);

        int PublishDue(DateTime now);

        int GetCommentCount(int articleId);

        Dictionary<int, int> CountPerCategory();
    }
}
