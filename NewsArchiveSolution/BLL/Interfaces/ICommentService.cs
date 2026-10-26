using BLL.DTOs;

namespace BLL.Interfaces
{
    public interface ICommentService
    {
        List<CommentDTO> Get();

        CommentDTO? Get(int id);

        bool Create(CommentDTO c);

        bool Update(CommentDTO c);

        bool Delete(int id);

        List<CommentDTO> GetByArticleId(int articleId, bool includeHidden = false);

        List<ReportReasonDTO> GetReasons();

        ReportResult Report(int commentId, int userId, int reasonId, string? note);

        HashSet<int> GetReportedIds(int userId, IEnumerable<int> commentIds);

        bool Hide(int commentId, int moderatorId);

        bool Keep(int commentId, int moderatorId);

        List<ModerationItemDTO> GetReported(int page, int pageSize, out int total);

        List<ModerationItemDTO> GetHidden(int page, int pageSize, out int total);

        int CountReported();
    }
}
