using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface IBookmarkRepository
    {
        bool Add(int userId, int articleId);

        bool Remove(int userId, int articleId);

        bool Exists(int userId, int articleId);

        int Count(int userId);

        HashSet<int> GetArticleIds(int userId, IEnumerable<int> articleIds);

        List<Article> GetArticles(int userId, int page, int pageSize, out int total);
    }
}
