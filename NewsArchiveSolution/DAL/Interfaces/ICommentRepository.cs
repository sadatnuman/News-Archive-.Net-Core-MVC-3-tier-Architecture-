using DAL.EF.Tables;

namespace DAL.Interfaces
{
    public interface ICommentRepository : IRepository<Comment>
    {
        List<Comment> GetByArticleId(int articleId, bool includeHidden = false);

        int CountVisible(int articleId);

        List<ReportReason> GetReasons();

        bool AddReport(int commentId, int userId, int reasonId, string? note, DateTime createdAt);

        int CountReports(int commentId);

        HashSet<int> GetReportedIds(int userId, IEnumerable<int> commentIds);

        bool SetHidden(int commentId, bool hidden, int? moderatorId, DateTime? moderatedAt);

        int ClearReports(int commentId);

        List<Comment> GetReported(int page, int pageSize, out int total);

        List<Comment> GetHidden(int page, int pageSize, out int total);

        int CountReported();
    }
}
