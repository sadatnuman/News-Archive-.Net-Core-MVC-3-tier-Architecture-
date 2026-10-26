using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface IRecommendationService
    {
        List<RecommendationDTO> ForArticle(int articleId, int count, int? userId = null, IEnumerable<int>? recentIds = null);

        List<RecommendationDTO> ForReader(int? userId, IEnumerable<int>? recentIds, string language, int count, IEnumerable<int>? excludeIds = null, int minHistory = 1);

        bool RecordRead(int userId, int articleId);

        List<int> GetRecentIds(int userId, int count);

        List<ReadingHistoryEntryDTO> GetHistory(int userId, int page, int pageSize, out int total);

        bool RemoveFromHistory(int userId, int articleId);

        int ClearHistory(int userId);
    }
}
