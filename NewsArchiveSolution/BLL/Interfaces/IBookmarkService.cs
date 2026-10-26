using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IBookmarkService
    {
        bool Add(int userId, int articleId);

        bool Remove(int userId, int articleId);

        bool Toggle(int userId, int articleId, out bool saved);

        bool IsSaved(int userId, int articleId);

        HashSet<int> GetSavedIds(int userId, IEnumerable<int> articleIds);

        List<ArticleDTO> GetSaved(int userId, int page, int pageSize, out int total);
    }
}
