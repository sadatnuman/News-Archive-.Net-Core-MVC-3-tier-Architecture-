using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface IReadingHistoryRepository
    {
        void Record(int userId, int articleId, DateTime readAt);

        void Trim(int userId, int keep);

        List<int> GetRecentArticleIds(int userId, int count);

        List<ReadingHistory> GetEntries(int userId, int page, int pageSize, out int total);

        bool Remove(int userId, int articleId);

        int Clear(int userId);

        int Count(int userId);
    }
}
