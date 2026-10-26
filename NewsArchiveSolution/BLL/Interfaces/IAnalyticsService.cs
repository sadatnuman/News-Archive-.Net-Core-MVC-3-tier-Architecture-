using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IAnalyticsService
    {
        int GetTotalArticles();

        int GetTotalUsers();

        List<ArticleDTO> GetMostViewedArticles(int count);

        List<(string Category, int Count)> GetArticlesPerCategory();
    }
}
