using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface IArticleRepository : IRepository<Article>
    {
        List<Article> GetByCategoryId(int categoryId, string? language, int page, int pageSize, out int total);

        Article? GetWithDetails(int id);

        void IncrementViewCount(int articleId);

        bool IsPublished(int articleId);

        bool UsesImage(string imagePath);

        void ReplaceWords(int articleId, IEnumerable<string> words);

        List<Article> GetUnindexed(int count);

        List<Article> Search(IReadOnlyList<string> terms, DateTime? from, DateTime? to, int page, int pageSize, out int total, string? language = null);

        List<Article> GetByPeriod(DateTime from, DateTime toExclusive, string? language, int page, int pageSize, out int total);

        List<(int Year, int Month, int Count)> CountByMonth(string? language = null);

        Dictionary<int, int> CountByDay(int year, int month, string? language = null);

        List<Article> GetOnThisDay(int month, int day, int beforeYear, int count, string? language = null);

        List<Article> GetBreaking(string? language = null);

        List<Article> GetTrending(int count, string? language = null);

        List<Article> GetMostViewed(int count);

        List<Article> GetLatest(int count, string? language = null);

        List<Article> GetRecommendationCandidates(string language, IEnumerable<int> excludeIds, IEnumerable<int> categoryIds,
            IEnumerable<int> tagIds, IEnumerable<int> authorIds, int relatedLimit, int popularLimit);

        List<Article> GetManyWithTags(IEnumerable<int> articleIds);

        List<Article> Suggest(string term, string? language, int count);

        List<Article> GetByAuthor(int authorId);

        List<Article> GetPublishedByAuthor(int authorId, string? language, int page, int pageSize, out int total);

        Dictionary<int, int> CountPublishedByAuthor(string? language = null);

        List<Article> GetByStatus(int statusId);

        List<Article> GetPublishedForSitemap(DateTime? since, int max);

        List<Article> GetWithoutSlug();

        int CountByStatus(int statusId);

        int PublishDue(DateTime now);

        Dictionary<int, int> CountPerCategory();
    }
}
